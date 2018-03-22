import { Job } from '../job/job';

export class Role {
  public id: string;
  public startDate?: Date;
  public active: boolean;
  public endDate?: Date;
  public personId: string;
  public jobId: string;
}

export class RoleExtended extends Role {
  public preferredName: string;
  public lastName: string;
}

export class RoleWithJob extends RoleExtended {
  public job: Job;
}
