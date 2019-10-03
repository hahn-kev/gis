import { BaseEntity } from '../../classes/base-entity';
import { JobType } from '../../job/job';

export enum TrainingScope {
  AllStaff = 'AllStaff',
  Department = 'Department'
}

export class TrainingRequirement extends BaseEntity {
  public name: string;
  public firstYear: number;
  public lastYear: number;
  public scope: TrainingScope;
  public jobScope: JobType[];
  public departmentId: string;
  public renewMonthsCount: number;
  public ownerId: string;
  public provider: string;

  constructor() {
    super();
    this.renewMonthsCount = 12;
    this.scope = TrainingScope.AllStaff;
    this.firstYear = new Date().getUTCFullYear();
  }
}
