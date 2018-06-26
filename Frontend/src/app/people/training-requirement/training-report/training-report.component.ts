import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { StaffTraining } from '../staff-training';
import { TrainingRequirementService } from '../training-requirement.service';
import { Year } from '../year';
import { RequirementWithStaff } from './requirement-with-staff';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, pluck, share } from 'rxjs/operators';
import { PersonService } from '../../person.service';
import * as moment from 'moment';
import { GroupService } from '../../groups/group.service';

@Component({
  selector: 'app-training-report',
  templateUrl: './training-report.component.html',
  styleUrls: ['./training-report.component.scss']
})
export class TrainingReportComponent implements OnInit {
  public years: Year[];
  public selectedYearSubject: BehaviorSubject<Year>;
  public selectedYear: Year;
  public expandedRequirementId: string;
  public completedDate = moment();
  public staffTraining = new BehaviorSubject<Map<string, StaffTraining>>(null);
  public requirementsWithStaff: Observable<RequirementWithStaff[]>;
  public showCompleted = new BehaviorSubject<boolean>(true);

  constructor(private route: ActivatedRoute,
              private trainingService: TrainingRequirementService,
              private personService: PersonService,
              private orgGroupService: GroupService,
              private router: Router) {
    this.years = Year.years();
    this.selectedYearSubject = new BehaviorSubject(null);
    this.route.params.pipe(
      pluck('year'),
      map(value => value || Year.CurrentSchoolYear()),
      map(yearValue => this.years.find(year => year.value == yearValue))
    ).subscribe(this.selectedYearSubject);
    this.selectedYearSubject.subscribe(year => {
      this.selectedYear = year;
      this.completedDate = year.convertToSchoolYear(this.completedDate);
    });
    this.route.queryParamMap.pipe(map(params => {
      return params.has('showCompleted') ? params.get('showCompleted') == 'true' : true;
    })).subscribe(this.showCompleted);
    let groups = this.orgGroupService.getAll().pipe(share());
    let staff = this.personService.getStaffWithRoles().pipe(share());
    let trainingRequirements = this.trainingService.list().pipe(share());

    this.route.data.pipe(pluck('staffTraining')).subscribe(this.staffTraining);
    this.requirementsWithStaff = this.trainingService.buildRequirementsWithStaff(staff,
      groups,
      trainingRequirements,
      this.staffTraining.asObservable(),
      this.selectedYearSubject.pipe(pluck('value')),
      this.showCompleted);

  }

  ngOnInit(): void {
  }

  setYear(year: number): void {
    this.updateNavigation(year, this.showCompleted.getValue());
  }

  get isLastYear(): boolean {
    return this.years[0].value === this.selectedYear.value;
  }

  get isFirstYear() {
    return this.years[this.years.length - 1].value === this.selectedYear.value;
  }

  setShowCompleted(show) {
    this.updateNavigation(this.selectedYear.value, show);
  }

  updateNavigation(year: number, showCompleted: boolean) {
    this.router.navigate([this.route.snapshot.params['year'] ? '..' : '.', year],
      {
        relativeTo: this.route,
        queryParams: {showCompleted: showCompleted}
      });
  }

  async completeTraining(reqObject: RequirementWithStaff, index: number): Promise<void> {
    const staffWithTraining = reqObject.staffsWithTraining[index];
    const staffTraining = new StaffTraining();
    staffTraining.trainingRequirementId = reqObject.requirement.id;
    staffTraining.staffId = staffWithTraining.staff.id;
    staffTraining.completedDate = this.completedDate.toDate();
    await this.trainingService.saveStaffTraining(staffTraining);
    this.trainingService.getStaffTrainingByYearMapped(this.selectedYear.value)
      .subscribe((training) => this.staffTraining.next(training));
  }

  async markSelectedComplete(reqObject: RequirementWithStaff) {
    const staffIds = reqObject.staffsWithTraining
      .filter(value => !value.training.completedDate && value.selected)
      .map(value => value.staff.id);
    await this.trainingService.markAllComplete(staffIds, reqObject.requirement.id, this.completedDate.toDate());
    this.trainingService.getStaffTrainingByYearMapped(this.selectedYear.value)
      .subscribe((training) => this.staffTraining.next(training));
  }

  async markAllComplete(reqObject: RequirementWithStaff): Promise<void> {
    const staffIds = reqObject.staffsWithTraining
      .filter(value => !value.training.completedDate)
      .map(value => value.staff.id);
    await this.trainingService.markAllComplete(staffIds, reqObject.requirement.id, this.completedDate.toDate());
    this.trainingService.getStaffTrainingByYearMapped(this.selectedYear.value)
      .subscribe((training) => this.staffTraining.next(training));
  }

  allSelected(reqObject: RequirementWithStaff) {
    let hasSelected = false;
    let hasNotSelected = false;
    for (let value of reqObject.staffsWithTraining.filter(st => !st.training.completedDate)) {
      if (value.selected) {
        hasSelected = true;
      } else {
        hasNotSelected = true;
      }
      if (hasNotSelected && hasSelected) return null;
    }
    return hasSelected && !hasNotSelected;
  }

  selectAll(reqObject: RequirementWithStaff, selected: boolean) {
    for (let value of reqObject.staffsWithTraining.filter(st => !st.training.completedDate)) {
      value.selected = selected;
    }
  }

}
