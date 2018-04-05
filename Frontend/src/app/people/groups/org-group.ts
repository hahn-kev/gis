import { BaseEntity } from '../../classes/base-entity';

export class OrgGroup extends BaseEntity {
  public groupName: string;
  public type: string;
  public supervisor: string;
  public parentId: string;
  public approverIsSupervisor: boolean;
}
