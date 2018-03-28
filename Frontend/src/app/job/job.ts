import { RoleExtended } from '../people/role';
import { BaseEntity } from '../classes/base-entity';

export class Job extends BaseEntity {
  public title: string;
  public type: JobType;
  public jobDescription: string;
  public gradeId: string;
  public orgGroupId: string;
  public current: boolean;
  public positions: number;

  constructor() {
    super();
    this.current = true;
  }

  static typeName(type: JobType): string {
    switch (type) {
      case JobType.FullTime:
        return 'Full Time';
      case JobType.HalfTime:
        return 'Half Time';
      case JobType.Contractor:
        return 'Contractor';
      case JobType.DailyWorker:
        return 'Daily Worker';
      case JobType.SchoolAid:
        return 'School Aid';
      case JobType.FullTime10Mo:
        return 'Full Time (10 month)';
      default:
        return type;
    }
  }
}

export class JobWithRoles extends Job {
  public roles: RoleExtended[];
}

export enum JobType {
  FullTime = 'FullTime',
  FullTime10Mo = 'FullTime10Mo',
  HalfTime = 'HalfTime',
  Contractor = 'Contractor',
  DailyWorker = 'DailyWorker',
  SchoolAid = 'SchoolAid',
}
