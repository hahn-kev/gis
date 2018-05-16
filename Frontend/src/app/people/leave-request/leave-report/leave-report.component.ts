import { Component, OnInit } from '@angular/core';
import { AppDataSource } from '../../../classes/app-data-source';
import { PersonAndLeaveDetails } from '../person-and-leave-details';
import { ActivatedRoute } from '@angular/router';
import { Person } from '../../person';
import { LeaveType, LeaveUseage } from '../../self/self';

@Component({
  selector: 'app-leave-report',
  templateUrl: './leave-report.component.html',
  styleUrls: ['./leave-report.component.scss']
})
export class LeaveReportComponent implements OnInit {
  public dataSource = new AppDataSource<PersonLeaveModel>();

  constructor(private route: ActivatedRoute) {
    this.dataSource.filterPredicate = ((data, filter) =>
      data.person.firstName.toUpperCase().startsWith(filter)
      || data.person.lastName.toUpperCase().startsWith(filter)
      || data.person.preferredName.toUpperCase().startsWith(filter)
    );
    this.route.data.subscribe((value: { people: PersonAndLeaveDetails[] }) => {
      this.dataSource.data = value.people.map(p => {
        let plm = new PersonLeaveModel();
        plm.person = p.person;

        for (let leave of p.leaveUseages) plm.appendLeave(leave);
        return plm;
      });
    });
  }

  ngOnInit() {
  }

  applyFilter(filterValue: string) {
    this.dataSource.filter = filterValue.trim().toUpperCase();
  }
}

export class PersonLeaveModel {
  public person: Person;
  public sick: LeaveUseage;
  public vacation: LeaveUseage;
  public personal: LeaveUseage;
  public parental: LeaveUseage;
  public emergency: LeaveUseage;
  public schoolRelated: LeaveUseage;
  public missionRelated: LeaveUseage;
  public other: LeaveUseage;

  appendLeave(leave: LeaveUseage) {
    switch (leave.leaveType) {
      case LeaveType.Sick:
        this.sick = leave;
        break;
      case LeaveType.Vacation:
        this.vacation = leave;
        break;
      case LeaveType.Personal:
        this.personal = leave;
        break;
      case LeaveType.Emergency:
        this.emergency = leave;
        break;
      case LeaveType.Maternity:
        if (this.person.gender == 'Female') this.parental = leave;
        break;
      case LeaveType.Paternity:
        if (this.person.gender == 'Male') this.parental = leave;
        break;
      case LeaveType.SchoolRelated:
        this.schoolRelated = leave;
        break;
      case LeaveType.MissionRelated:
        this.missionRelated = leave;
        break;
      default:
        if (!this.other) {
          this.other = new LeaveUseage();
          this.other.leaveType = LeaveType.Other;
        }
        this.other.totalAllowed += leave.totalAllowed;
        this.other.left += leave.left;
        this.other.used += leave.used;
    }
  }
}
