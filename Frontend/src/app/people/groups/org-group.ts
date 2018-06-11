import { BaseEntity } from '../../classes/base-entity';
import { PersonWithStaff } from '../person';

export class OrgGroup extends BaseEntity {
  public groupName: string;
  public type: string;
  public supervisor: string;
  public parentId: string;
  public approverIsSupervisor: boolean;
}

export class OrgGroupWithSupervisor extends OrgGroup {
  public supervisorPerson: PersonWithStaff;
}
