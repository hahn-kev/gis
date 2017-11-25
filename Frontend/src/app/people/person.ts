export class Person {

  constructor(public firstName?: string, public lastName?: string, public id?: string) {
  }
}

export class PersonExtended extends Person {

  constructor(FirstName?: string,
              LastName?: string,
              Id?: string,
              public speaksEnglish?: boolean,
              public isThai?: boolean) {
    super(FirstName, LastName, Id);
  }
}
