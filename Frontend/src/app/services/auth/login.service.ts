import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { User } from '../../user/user';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { LocalStorageService } from 'angular-2-local-storage';
import { JwtHelperService } from './jwt-helper.service';
import { map } from 'rxjs/operators';

@Injectable()
export class LoginService implements CanActivate {
  private readonly currentUserSubject = new BehaviorSubject<User>(null);
  public redirectTo: string;
  public accessToken: string;
  public rolesSubject = new BehaviorSubject<string[]>([]);

  constructor(private router: Router, private localStorage: LocalStorageService) {
    this.redirectTo = router.routerState.snapshot.url;
    this.setLoggedIn(localStorage.get<User>('user'), localStorage.get<string>('accessToken'));
  }

  promptLogin(redirectTo?: string) {
    this.redirectTo = redirectTo || this.router.routerState.snapshot.url;
    this.router.navigate(['/login']);
  }

  setLoggedIn(user: User, accessToken: string) {

    this.localStorage.set('accessToken', accessToken);
    this.localStorage.set('user', user);
    this.accessToken = accessToken;
    let decodedToken = JwtHelperService.decodeToken(accessToken);
    //todo pull out roles, and any claims we want
    let roles = [];
    if (decodedToken) {
      roles.push(decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']);
      //exp is expiration date in seconds from epoch
      let expireDate = new Date(decodedToken.exp * 1000);
      if (expireDate.valueOf() < new Date().valueOf()) {
        user = null;
        roles = [];
        this.promptLogin();
        return false;
      }
      //todo if user is null fetch user from 'sub' in token
    }
    this.rolesSubject.next(roles);
    this.currentUserSubject.next(user);
    return true;
  }

  loggedIn(): Observable<boolean> {
    return this.currentUserSubject.pipe(map((user) => user != null && this.accessToken != null));
  }

  observeCurrentUser() {
    return this.currentUserSubject.asObservable();
  }

  currentUser(): User {
    return this.currentUserSubject.getValue();
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    return this.loggedIn().map(loggedIn => {
      if (!loggedIn) {
        this.promptLogin(state.url);
      }
      return loggedIn;
    });
  }

  hasRole(role: string): Observable<boolean> {
    return this.rolesSubject.map(roles => roles.includes(role));
  }
}
