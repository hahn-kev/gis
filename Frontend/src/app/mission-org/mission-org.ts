import { BaseEntity } from '../classes/base-entity';

export class MissionOrg extends BaseEntity {
  public name: string;
  public repId: string;
  public phone: string;
  public email: string;
  public address: string;
  public officeInThailand: boolean;
}
