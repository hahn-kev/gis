import { isArray } from 'util';

export class UserToken {

  constructor(private token: any) {
  }


  get userName(): string {
    return this.token && this.token['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
  }

  get expirationDate(): Date {
    if (!this.token) return new Date();
    //exp is expiration date in seconds from epoch
    return new Date(this.token.exp * 1000);
  }

  get email(): string {
    return this.token && this.token['email'];
  }
  get roles(): string[] {
    let val = this.token['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (isArray(val)) return val;
    return [val];
  }
}
