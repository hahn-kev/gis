import { RoleExtended } from '../people/role';
import { BaseEntity } from '../classes/base-entity';

export class Job extends BaseEntity {
  public title: string;
  public type: string;
  public jobDescription: string;
  public orgGroupId: string;
  public current: boolean;
  public positions: number;

  constructor() {
    super();
    this.current = true;
  }
}

export class JobWithRoles extends Job {
  public roles: RoleExtended[];
}
