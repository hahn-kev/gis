import { Person } from '../../person';
import { LeaveType, LeaveUsage } from '../../self/self';

export class PersonLeaveModel {
  public person: Person;
  public sick: LeaveUsage;
  public vacation: LeaveUsage;
  public personal: LeaveUsage;
  public parental: LeaveUsage;
  public emergency: LeaveUsage;
  public schoolRelated: LeaveUsage;
  public missionRelated: LeaveUsage;
  public other: LeaveUsage;

  appendLeave(leave: LeaveUsage) {
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
          this.other = new LeaveUsage();
          this.other.leaveType = LeaveType.Other;
        }
        this.other.totalAllowed += leave.totalAllowed;
        this.other.left += leave.left;
        this.other.used += leave.used;
    }
  }
}
