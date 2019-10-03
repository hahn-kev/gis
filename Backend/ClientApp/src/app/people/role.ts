import { JobWithOrgGroup } from '../job/job';
import { BaseEntity } from '../classes/base-entity';

export class Role extends BaseEntity {
  public startDate?: Date;
  public active: boolean;
  public endDate?: Date;
  public personId: string;
  public jobId: string;
  public notes: string;
}

export class RoleExtended extends Role {
  public preferredName: string;
  public lastName: string;
}

export class RoleWithJob extends RoleExtended {
  public job: JobWithOrgGroup;
}
