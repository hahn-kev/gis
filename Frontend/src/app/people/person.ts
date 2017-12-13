import { Role } from './role';

export class Person {

  constructor(public firstName?: string,
              public lastName?: string,
              public id?: string,
              public email?: string,
              public staffId?: string) {
  }
}

export class PersonExtended extends Person {
  public speaksEnglish: boolean;
  public isThai: boolean;
  public staff: Staff;
  public preferredName: string;
}

export class PersonWithOthers extends PersonExtended {

  public roles: Role[] = [];
}

export class Staff {
  public id: string;
  public orgGroupId: string;
}

export class StaffWithName extends Staff {
  public preferredName: string;
  public personId: string;
}
