import { PersonWithOthers } from '../person';

export class Self {
  public person: PersonWithOthers;
  public leaveDetails: LeaveDetails;
}

export class LeaveDetails {
  public leaveUseages: LeaveUseage[];
}

export class LeaveUseage {
  public leaveType: string;
  public used: number;
  public totalAllowed: number;
  public left: number;
}

export enum LeaveType {
  Vacation = 'Vacation',
  Sick = 'Sick',
  Personal = 'Personal',
  Maternity = 'Maternity',
  Paternity = 'Paternity',
  Emergency = 'Emergency',
  SchoolRelated = 'SchoolRelated',
  Other = 'Other',
}
