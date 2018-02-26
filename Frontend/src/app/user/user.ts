export class User {
  constructor(public userName?: string,
              public phoneNumber?: string,
              public email?: string,
              public id?: number,
              public isAdmin = false,
              public resetPassword = false,
              public personId?: string) {
  }

  public isHr: boolean;
}
