import { BaseEntity } from '../classes/base-entity';

export class Donor extends BaseEntity {
  public notes: string;
  public status = DonorStatus.Unknown;
  public isBigDonor = false;
}

export enum DonorStatus {
  Unknown = 'Unknown',
  Active = 'Active',
  Inactive = 'Inactive'
}

export class Donation extends BaseEntity {
  public date: Date;
  public money: number;
  public personId: string;
  public missionOrgId: string;
}
