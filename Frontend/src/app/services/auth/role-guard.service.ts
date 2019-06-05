import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { LoginService } from './login.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { tap } from 'rxjs/operators';

@Injectable()
export class RoleGuardService implements CanActivate {

  constructor(private loginService: LoginService, private snackBarService: MatSnackBar) {
  }

  canActivate(route: ActivatedRouteSnapshot,
              state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    //noinspection TypeScriptUnresolvedVariable
    let role = route.data.requireRole;
    if (!role) {
      console.warn(`RoleGuardService used on ${route.url} without a role being set, letting user through`);
      return true;
    }
    let allowedObservable = typeof role === 'string' ?
      this.loginService.hasRole(role) : this.loginService.hasAnyRole(role);
    return allowedObservable.pipe(tap(hasRole => {
      if (!hasRole) {
        this.snackBarService.open(`Access denied, missing role [${role}]`, 'dismiss', {duration: 2000});
      }
    }));
  }

}
