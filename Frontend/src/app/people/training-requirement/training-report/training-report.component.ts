import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TrainingRequirement } from '../training-requirement';
import { StaffTraining } from '../staff-training';
import { OrgGroup } from '../../groups/org-group';
import { Staff, StaffWithName } from '../../person';

@Component({
  selector: 'app-training-report',
  templateUrl: './training-report.component.html',
  styleUrls: ['./training-report.component.scss']
})
export class TrainingReportComponent implements OnInit {
  public staffTraining: StaffTraining[];
  public trainingRequirements: TrainingRequirement[];
  public orgGroups: OrgGroup[];
  public staff: StaffWithName[]

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.route.data.subscribe((value: {
      trainingRequirements: TrainingRequirement[],
      staffTraining: StaffTraining[],
      groups: OrgGroup[],
      staff: StaffWithName[]
    }) => {
      this.staffTraining = value.staffTraining;
      this.trainingRequirements = value.trainingRequirements;
      this.orgGroups = value.groups;
      this.staff = value.staff;
    });

  }

  private filterYear: number;
//todo use observables to deal with filtering
}
