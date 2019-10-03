import { RoleExtended } from '../people/role';
import { BaseEntity } from '../classes/base-entity';
import { OrgGroup } from '../people/groups/org-group';
import { RequiredEndorsementWithName } from '../endorsement/endorsement';

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
  public gradeNo: number;
}

export class JobWithRoles extends Job {
  public roles: RoleExtended[] = [];
  public requiredEndorsements: RequiredEndorsementWithName[] = [];
}

export enum JobStatus {
  FullTime = 'FullTime',
  FullTime10Mo = 'FullTime10Mo',
  HalfTime = 'HalfTime',
  Contractor = 'Contractor',
  DailyWorker = 'DailyWorker',
  SchoolAid = 'SchoolAid',
}

export const NonSchoolAidJobStatus = Object.keys(JobStatus)
  .map(value => JobStatus[value])
  .filter(value => value != JobStatus.SchoolAid);

export enum JobType {
  Admin = 'Admin',
  Support = 'Support',
  Teacher = 'Teacher',
  BlueCollar = 'BlueCollar'
}

export const AllJobTypes = Object.keys(JobType)
  .map(value => JobType[value]);

