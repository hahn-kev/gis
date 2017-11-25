import { Role } from './role';

export class Person {

  constructor(public firstName?: string, public lastName?: string, public id?: string) {
  }
}

export class PersonExtended extends Person {

  constructor(FirstName?: string,
    LastName?: string,
    Id?: string,
    public speaksEnglish?: boolean,
    public isThai?: boolean,
    public roles: Role[] = []) {
    super(FirstName, LastName, Id);
  }
}
