import { isArray } from 'util';

export class UserToken {

  constructor(private token: { [key: string]: any }) {
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
    const val = this.token['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (isArray(val)) return val;
    return [val];
  }

  get personId(): string | null {
    if (!this.token) return null;
    return this.token['personId'];
  }

  get oauth(): string | null {
    if (!this.token) return null;
    return this.token['oauth'];
  }

  get isSupervisor(): boolean {
    if (!this.token) return false;
    return !!this.token['supervisesGroupId'];
  }

  get orgGroupId(): string {
    if (!this.isSupervisor) return null;
    return this.token['supervisesGroupId'];
  }

  hasAnyRole(roles: string[]) {
    return roles.some(value => this.roles.includes(value));
  }

  hasRole(role) {
    return this.roles.includes(role);
  }

  isHrOrAdmin() {
    return this.hasAnyRole(['admin', 'hr']);
  }
}
