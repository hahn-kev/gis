import { BaseEntity } from '../classes/base-entity';

export class MissionOrg extends BaseEntity {
  public name: string;
  public repId: string;
  public phone: string;
  public email: string;
  public address: string;
  public addressLocal: string;
  public officeInThailand: boolean;
  public approvedDate: Date;
  public status?: MissionOrgStatus;
}

export class MissionOrgWithNames extends MissionOrg {
  public contactName: string;
}


export enum MissionOrgStatus {
  Associate = 'Associate',
  OwnerAssociate = 'OwnerAssociate',
  FoundingAssociate = 'FoundingAssociate',
  Founder = 'Founder'
}
