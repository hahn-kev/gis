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
  public selectedYear: BehaviorSubject<Year>;
  public activeYear: Year;
  public year: number;
  public expandedRequirementId: string;
  public completedDate = new Date();
  public staffTraining = new BehaviorSubject<Map<string, StaffTraining>>(null);
  public requirementsWithStaff: Observable<RequirementWithStaff[]>;

  constructor(private route: ActivatedRoute,
              private trainingService: TrainingRequirementService,
              private personService: PersonService,
              private router: Router) {
    this.years = this.trainingService.years();
    this.selectedYear = new BehaviorSubject(null);
    this.route.params.pipe(
      pluck('year'),
      map(value => value || new Date().getUTCFullYear()),
      map(yearValue => this.years.find(year => year.value === yearValue))
    ).subscribe(this.selectedYear);
    this.selectedYear.subscribe(year => this.activeYear = year);
  }

  ngOnInit(): void {
    this.route.data.pipe(pluck('staffTraining')).subscribe(this.staffTraining);
    this.requirementsWithStaff = this.trainingService.buildRequirementsWithStaff(this.personService.getStaff(),
      this.trainingService.list(),
      this.staffTraining.asObservable(),
      this.selectedYear.pipe(pluck('value')));
  }

  setYear(year: number): void {
    this.router.navigate([this.route.snapshot.params['year'] ? '..' : '.', year],
      {
        relativeTo: this.route,
      });
  }

  async completeTraining(reqObject: RequirementWithStaff, index: number): Promise<void> {
    const staffWithTraining = reqObject.staffsWithTraining[index];
    const staffTraining = new StaffTraining();
    staffTraining.trainingRequirementId = reqObject.requirement.id;
    staffTraining.staffId = staffWithTraining.staff.id;
    staffTraining.completedDate = this.completedDate;
    await this.trainingService.saveStaffTraining(staffTraining);
    this.trainingService.getStaffTrainingByYearMapped(this.selectedYear.getValue().value).subscribe(this.staffTraining);
  }

  async markAllComplete(reqObject: RequirementWithStaff): Promise<void> {
    const staffIds = reqObject.staffsWithTraining
      .filter(value => !value.training.completedDate)
      .map(value => value.staff.id);
    await this.trainingService.markAllComplete(staffIds, reqObject.requirement.id, this.completedDate);
    this.trainingService.getStaffTrainingByYearMapped(this.selectedYear.getValue().value).subscribe(this.staffTraining);
  }
}
