import { BaseEntity } from '../../classes/base-entity';
import { PersonWithStaff } from '../person';

export class OrgGroup extends BaseEntity {
  public groupName: string;
  public type: GroupType;
  public supervisor: string;
  public parentId: string;
  public approverIsSupervisor: boolean;
}

export class OrgGroupWithSupervisor extends OrgGroup {
  public supervisorPerson: PersonWithStaff;
}

export enum GroupType {
  Division = 'Division',
  Department = 'Department',
  Supervisor = 'Supervisor'
}
