import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { JwtHelperService } from './jwt-helper.service';
import { filter, map } from 'rxjs/operators';
import * as Raven from 'raven-js';
import { CookieService } from 'ngx-cookie';
import { UserToken } from '../../login/user-token';

@Injectable()
export class LoginService implements CanActivate {
  private readonly currentUserTokenSubject = new BehaviorSubject<string>(null);
  public redirectTo: string;

  constructor(private router: Router, private cookieService: CookieService) {
    this.redirectTo = router.routerState.snapshot.url;
    this.setLoggedIn(cookieService.get('.JwtAccessToken'));
    this.currentUserToken().subscribe(user => {
      if (user) {
        Raven.setUserContext({username: user.userName});
      } else {
        Raven.setUserContext();
      }
    });
    // this.currentUserToken().subscribe(user => {
    //   if ()
    //     this.promptLogin();
    // })
  }

  promptLogin(redirectTo?: string): void {
    this.redirectTo = redirectTo || this.router.routerState.snapshot.url;
    this.router.navigate(['/login']);
  }

  setLoggedIn(accessToken: string): void {
    this.currentUserTokenSubject.next(accessToken);
  }

  loggedIn(): Observable<boolean> {
    return this.currentUserToken().pipe(map((user) => user && user.expirationDate.valueOf() > new Date().valueOf()));
  }

  currentUserToken(): Observable<UserToken> {
    return this.currentUserTokenSubject.pipe(map(jwt => jwt && new UserToken(JwtHelperService.decodeToken(jwt))));
  }

  safeUserToken(): Observable<UserToken> {
    return this.currentUserToken().pipe(filter(value => !!value));
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any> {
    return this.loggedIn().map(loggedIn => {
      if (!loggedIn) {
        this.promptLogin(state.url);
      }
      return loggedIn;
    });
  }

  isHrOrAdmin() {
    return this.hasAnyRole(['admin', 'hr']);
  }

  hasAnyRole(roles: string[]) {
    return this.currentUserToken().pipe(map(user => user && roles.some(role => user.roles.includes(role))));
  }

  hasRole(role: string): Observable<boolean> {
    return this.currentUserToken().pipe(map(user => user && user.roles.includes(role)));
  }
}
