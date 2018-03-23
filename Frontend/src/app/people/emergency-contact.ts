import { BaseEntity } from '../classes/base-entity';

export class EmergencyContact extends BaseEntity {
  public personId: string;
  public contactId: string;
  public order: number;
  public relationship: string;
}

export class EmergencyContactExtended extends EmergencyContact {
  public contactPreferedName: string;
  public contactLastName: string;
}
