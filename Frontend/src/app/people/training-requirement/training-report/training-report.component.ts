import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TrainingRequirement } from '../training-requirement';
import { StaffTraining } from '../staff-training';
import { OrgGroup } from '../../groups/org-group';
import { Staff, StaffWithName } from '../../person';
import { TrainingRequirementService } from '../training-requirement.service';
import { Year } from '../year';
import { RequirementWithStaff, StaffWithTraining } from './requirement-with-staff';

@Component({
  selector: 'app-training-report',
  templateUrl: './training-report.component.html',
  styleUrls: ['./training-report.component.scss']
})
export class TrainingReportComponent implements OnInit {
  public staffTraining: Map<string, StaffTraining>;
  public trainingRequirements: TrainingRequirement[];
  public orgGroups: OrgGroup[];
  public staff: StaffWithName[];
  public years: Year[];
  public selectedYear: Year;
  public expandedRequirementId: string;
  public completedDate = new Date();

  constructor(private route: ActivatedRoute,
              private trainingService: TrainingRequirementService,
              private router: Router) {
    this.years = this.trainingService.years();
    this.route.params.subscribe(params => {
      let yearValue = params['year'] || new Date().getUTCFullYear();
      this.selectedYear = this.years.find(year => year.value == yearValue);
    });
  }

  ngOnInit() {
    this.route.data.subscribe((value: {
      trainingRequirements: TrainingRequirement[],
      staffTraining: StaffTraining[],
      groups: OrgGroup[],
      staff: StaffWithName[]
    }) => {

      this.setStaffTraining(value.staffTraining);
      this.trainingRequirements = value.trainingRequirements;
      this.orgGroups = value.groups;
      this.staff = value.staff;
      this.buildList();
    });
  }

  setStaffTraining(trainingList: StaffTraining[]) {
    let map = trainingList.map((training): [string, StaffTraining] => [
      StaffTraining.getKey(training),
      training
    ]);
    this.staffTraining = new Map<string, StaffTraining>(map);
  }

  setYear(year: number) {
    this.router.navigate([this.route.snapshot.params['year'] ? '..' : '.', year],
      {
        relativeTo: this.route,
      });
  }

  public requirementsWithStaff: RequirementWithStaff[];

  buildList() {
    this.requirementsWithStaff = this.trainingRequirements
      .filter(requirement => requirement.firstYear <= this.selectedYear.value
        && (!requirement.lastYear || requirement.lastYear >= this.selectedYear.value))
      .map(requirement => {
        return new RequirementWithStaff(requirement, this.buildStaffWithTrainingList(requirement));
      });
  }

  buildStaffWithTrainingList(requirement: TrainingRequirement) {
    return this.staff.map(staff =>
      new StaffWithTraining(staff, this.staffTraining.get(staff.id + '_' + requirement.id))
    );
  }

  async completeTraining(reqObject: RequirementWithStaff, index: number) {
    let staffWithTraining = reqObject.staffsWithTraining[index];
    let staffTraining = new StaffTraining();
    staffTraining.trainingRequirementId = reqObject.requirement.id;
    staffTraining.staffId = staffWithTraining.staff.id;
    staffTraining.completedDate = this.completedDate;
    staffTraining = await this.trainingService.saveStaffTraining(staffTraining);
    this.staffTraining.set(StaffTraining.getKey(staffTraining), staffTraining);
    this.buildList();
  }

  async markAllComplete(reqObject: RequirementWithStaff) {
    let staffIds = reqObject.staffsWithTraining
      .filter(value => !value.training.completedDate)
      .map(value => value.staff.id);
    await this.trainingService.markAllComplete(staffIds, reqObject.requirement.id, this.completedDate);
    this.setStaffTraining(await this.trainingService.getStaffTrainingByYear(this.selectedYear.value).toPromise());
    this.buildList();
  }
}
