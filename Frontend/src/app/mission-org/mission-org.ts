import { BaseEntity } from '../classes/base-entity';
import { MissionOrgYearSummary } from './mission-org-year-summary';

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

export class MissionOrgWithYearSummaries extends MissionOrg {
  public yearSummaries: MissionOrgYearSummary[] = [];
}


export enum MissionOrgStatus {
  Associate = 'Associate',
  OwnerAssociate = 'OwnerAssociate',
  FoundingAssociate = 'FoundingAssociate',
  Founder = 'Founder'
}
