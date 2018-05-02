import { RoleWithJob } from './role';
import { EmergencyContactExtended } from './emergency-contact';
import { BaseEntity } from '../classes/base-entity';
import { LeaveDetails } from './self/self';
import { StaffWithOrgName } from './staff';
import { EvaluationWithNames } from './person/evaluation/evaluation';

export class Person extends BaseEntity {
  public firstName: string;
  public lastName: string;
  public email: string;
  public staffId?: string;

  public preferredName: string;
  public gender = Gender.Male;
  public isThai: boolean;
  public isSchoolAid: boolean;
}

export enum Gender {
  Male = 'Male',
  Female = 'Female'
}

export class PersonExtended extends Person {
  public thaiFirstName: string;
  public thaiLastName: string;
  public speaksEnglish: boolean;
  public phoneNumber: string;
  public spouseId: string;
  public spouseChanged: boolean;

  public nationality?: Nationality;
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
  public staff: StaffWithOrgName;
  public spousePreferedName: string;
}

export class PersonWithStaffSummaries extends PersonWithStaff {
  public startDate: Date;
  public daysOfService: number;
  public isActive: boolean;
}

export class PersonWithOthers extends PersonWithStaff {
  public leaveDetails: LeaveDetails;
  public roles: RoleWithJob[] = [];
  public emergencyContacts: EmergencyContactExtended[] = [];
  public evaluations: EvaluationWithNames[] = [];
}

export class PersonWithDaysOfLeave extends Person {
  public sickDaysOfLeaveUsed: number;
  public vacationDaysOfLeaveUsed: number;
}

export enum Nationality {
  NorthAmerica = 'NorthAmerica',
  CentralSouthAmerica = 'CentralSouthAmerica',
  Africa = 'Africa',
  MiddleEast = 'MiddleEast',
  Europe = 'Europe',
  CentralAsia = 'CentralAsia',
  EastAsia = 'EastAsia',
  Oceania = 'Oceania',
}

export function NationalityName(nationality: Nationality): string {
  switch (nationality) {
    case Nationality.NorthAmerica:
      return 'North America';
    case Nationality.CentralSouthAmerica:
      return 'Central South America';
    case Nationality.MiddleEast:
      return 'Middle East';
    case Nationality.CentralAsia:
      return 'Central Asia';
    case Nationality.EastAsia:
      return 'East Asia';
    default:
      return nationality;
  }
}
