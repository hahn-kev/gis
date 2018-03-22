import { Role, RoleExtended, RoleWithJob } from './role';
import { EmergencyContactExtended } from './emergency-contact';

export class Person {

  constructor(public firstName?: string,
              public lastName?: string,
              public id?: string,
              public email?: string,
              public staffId?: string) {
  }

  public preferredName: string;
  public gender = 'Male';
  public isThai: boolean;
}

export class PersonExtended extends Person {
  public speaksEnglish: boolean;
  public phoneNumber: string;
  public spouseId: string;
  public spouseChanged: boolean;

  public nationality: string;
  public birthdate?: Date;

  public passportAddress: string;
  public passportCity: string;
  public passportState: string;
  public passportZip: string;
  public passportCountry: string;

  public thaiAddress: string;
  public thaiSoi: string;
  public thaiMubaan: string;
  public thaiTambon: string;
  public thaiAmphur: string;
  public thaiProvince: string;
  public thaiZip: string;
}

export class PersonWithStaff extends PersonExtended {
  public staff: Staff;
  public spousePreferedName: string;
}

export class PersonWithOthers extends PersonWithStaff {

  public roles: RoleWithJob[] = [];
  public emergencyContacts: EmergencyContactExtended[] = [];
}

export class PersonWithDaysOfLeave extends Person {
  public sickDaysOfLeaveUsed: number;
  public vacationDaysOfLeaveUsed: number;
}

export class Staff {
  public id: string;
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
  public thaiSsn: string;
  public visaType: string;
  public workPermitType: string;

  public endorsements: string;
  public endorsementAgency: string;

  constructor(id?: string) {
    this.id = id;
  }
}

export class StaffWithName extends Staff {
  public preferredName: string;
  public personId: string;

  constructor(id?: string, preferredName?: string) {
    super(id);
    this.preferredName = preferredName;
  }
}
