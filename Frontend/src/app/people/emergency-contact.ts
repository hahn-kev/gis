import { BaseEntity } from '../classes/base-entity';

export class EmergencyContact extends BaseEntity {
  public personId: string;
  public contactId: string;
  public order: number;
  public relationship: string;

  //these are only used when contactId is null
  public name: string;
  public phone: string;
  public email: string;
}

export class EmergencyContactExtended extends EmergencyContact {
  public contactPreferredName: string;
  public contactLastName: string;
}
