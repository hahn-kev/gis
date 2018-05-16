import { PersonWithOthers } from '../person';

export class Self {
  public person: PersonWithOthers;
  public leaveDetails: LeaveDetails;
}

export class LeaveDetails {
  public leaveUseages: LeaveUseage[];
}

export class LeaveUseage {
  public leaveType: LeaveType;
  public used = 0;
  public totalAllowed = 0;
  public left = 0;
}

export enum LeaveType {
  Vacation = 'Vacation',
  Sick = 'Sick',
  Personal = 'Personal',
  Maternity = 'Maternity',
  Paternity = 'Paternity',
  Emergency = 'Emergency',
  SchoolRelated = 'SchoolRelated',
  MissionRelated = 'MissionRelated',
  Other = 'Other',
}

export function LeaveTypeName(type: LeaveType): string {
  switch (type) {
    case LeaveType.SchoolRelated:
      return 'School Related';
    case LeaveType.MissionRelated:
      return 'Mission Related';
    default:
      return type;
  }
}
