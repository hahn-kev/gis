import { BaseEntity } from '../classes/base-entity';

export class Staff extends BaseEntity {
  public email: string;
  public phoneExt: string;
  public orgGroupId: string;
  public missionOrgId: string;
  public annualSalary: number;
  public renwebId: string;
  public moeLicenseNumber: string;
  public contractIssued?: Date;
  public contractExpireDate?: Date;
  public insuranceNumber: string;
  public teacherLicenseOrg: string;
  public teacherLicenseNo: string;
  public visaType: string;
  public workPermitType: string;
  public endorsements: string;
  public endorsementAgency: string;

  constructor(id?: string) {
    super();
    this.id = id;
  }
}

export class StaffWithName extends Staff {
  public preferredName: string;
  public lastName: string;
  public personId: string;

  constructor(id?: string, preferredName?: string) {
    super(id);
    this.preferredName = preferredName;
  }
}

export class StaffWithOrgName extends Staff {
  public missionOrgName: string;
  public missionOrgEmail: string;
}
