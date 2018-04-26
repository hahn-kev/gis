import { RoleExtended } from '../people/role';
import { BaseEntity } from '../classes/base-entity';
import { OrgGroup } from '../people/groups/org-group';

export class Job extends BaseEntity {
  public title: string;
  public type: JobType;
  public status: JobStatus;
  public jobDescription: string;
  public gradeId: string;
  public orgGroupId: string;
  public current: boolean;
  public positions: number;

  constructor() {
    super();
    this.current = true;
  }
}

export class JobWithFilledInfo extends Job {
  public filled: number;
  public open: number;
  public gradeNo: number;
  public orgGroupName: string;
}

export class JobWithOrgGroup extends Job {
  public orgGroup: OrgGroup;
}

export class JobWithRoles extends Job {
  public roles: RoleExtended[];
}

export enum JobStatus {
  FullTime = 'FullTime',
  FullTime10Mo = 'FullTime10Mo',
  HalfTime = 'HalfTime',
  Contractor = 'Contractor',
  DailyWorker = 'DailyWorker',
  SchoolAid = 'SchoolAid',
}

export var NonSchoolAidJobStatus = Object.keys(JobStatus)
  .map(value => JobStatus[value])
  .filter(value => value != JobStatus.SchoolAid);

export function jobStatusName(status: JobStatus): string {
  switch (status) {
    case JobStatus.FullTime:
      return 'Full Time';
    case JobStatus.HalfTime:
      return 'Half Time';
    case JobStatus.DailyWorker:
      return 'Daily Worker';
    case JobStatus.SchoolAid:
      return 'School Aid';
    case JobStatus.FullTime10Mo:
      return 'Full Time (10 month)';
    default:
      return status;
  }
}

export enum JobType {
  Admin = 'Admin',
  Support = 'Support',
  Teacher = 'Teacher',
  BlueCollar = 'BlueCollar'
}

export function jobTypeName(type: JobType) {
  switch (type) {
    case JobType.BlueCollar:
      return 'Blue Collar';
    default:
      return type;
  }
}
