import { Role } from './role';

export class Person {

  constructor(public firstName?: string,
              public lastName?: string,
              public id?: string,
              public email?: string,
              public staffId?: string) {
  }

  public preferredName: string;
}

export class PersonExtended extends Person {
  public speaksEnglish: boolean;
  public isThai: boolean;
  public country: string;
  public phoneNumber: string;
  public spouseId: string;
  public spouseChanged: boolean;
}

export class PersonWithStaff extends PersonExtended {
  public staff: Staff;
  public spousePreferedName: string;
}

export class PersonWithOthers extends PersonWithStaff {

  public roles: Role[] = [];
}

export class PersonWithDaysOfLeave extends Person {
  public sickDaysOfLeaveUsed: number;
  public vacationDaysOfLeaveUsed: number;
}

export class Staff {

  constructor(id?: string) {
    this.id = id;
  }

  public id: string;
  public orgGroupId: string;
}

export class StaffWithName extends Staff {

  constructor(id?: string, preferredName?: string) {
    super(id);
    this.preferredName = preferredName;
  }

  public preferredName: string;
  public personId: string;
}
