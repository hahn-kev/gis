import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { StaffTraining } from '../staff-training';
import { TrainingRequirementService } from '../training-requirement.service';
import { Year } from '../year';
import { RequirementWithStaff } from './requirement-with-staff';
import { Observable } from 'rxjs/Observable';
import { map, pluck } from 'rxjs/operators';
import { PersonService } from '../../person.service';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

@Component({
  selector: 'app-training-report',
  templateUrl: './training-report.component.html',
  styleUrls: ['./training-report.component.scss']
})
export class TrainingReportComponent implements OnInit {
  public years: Year[];
  public selectedYearSubject: BehaviorSubject<Year>;
  public selectedYear: Year;
  public year: number;
  public expandedRequirementId: string;
  public completedDate = new Date();
  public staffTraining = new BehaviorSubject<Map<string, StaffTraining>>(null);
  public requirementsWithStaff: Observable<RequirementWithStaff[]>;
  public showCompleted = new BehaviorSubject<boolean>(true);

  constructor(private route: ActivatedRoute,
              private trainingService: TrainingRequirementService,
              private personService: PersonService,
              private router: Router) {
    this.years = this.trainingService.years();
    this.selectedYearSubject = new BehaviorSubject(null);
    this.route.params.pipe(
      pluck('year'),
      map(value => value || new Date().getUTCFullYear()),
      map(yearValue => this.years.find(year => year.value == yearValue))
    ).subscribe(this.selectedYearSubject);
    this.selectedYearSubject.subscribe(year => this.selectedYear = year);
    this.route.queryParamMap.pipe(map(params => {
      return params.has('showCompleted') ? params.get('showCompleted') == 'true' : true;
    })).subscribe(this.showCompleted);

    this.route.data.pipe(pluck('staffTraining')).subscribe(this.staffTraining);
    this.requirementsWithStaff = this.trainingService.buildRequirementsWithStaff(this.personService.getStaff(),
      this.trainingService.list(),
      this.staffTraining.asObservable(),
      this.selectedYearSubject.pipe(pluck('value')),
      this.showCompleted);
  }

  ngOnInit(): void {
  }

  setYear(year: number): void {
    this.updateNavigation(year, this.showCompleted.getValue());
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
    staffTraining.completedDate = this.completedDate;
    await this.trainingService.saveStaffTraining(staffTraining);
    this.trainingService.getStaffTrainingByYearMapped(this.selectedYear.value).subscribe(this.staffTraining);
  }

  async markAllComplete(reqObject: RequirementWithStaff): Promise<void> {
    const staffIds = reqObject.staffsWithTraining
      .filter(value => !value.training.completedDate)
      .map(value => value.staff.id);
    await this.trainingService.markAllComplete(staffIds, reqObject.requirement.id, this.completedDate);
    this.trainingService.getStaffTrainingByYearMapped(this.selectedYear.value).subscribe(this.staffTraining);
  }
}
