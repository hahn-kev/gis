import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { LoginService } from './login.service';
import { MatSnackBar } from '@angular/material';
import { tap } from 'rxjs/operators';

@Injectable()
export class RoleGuardService implements CanActivate {
  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    //noinspection TypeScriptUnresolvedVariable
    let role = route.data.requireRole;
    if (!role) {
      return true;
    }

    return this.loginService.hasRole(role).pipe(tap(hasRole => {
      if (!hasRole) {
        this.snackBarService.open(`Access denied, missing role [${role}]`, 'dismiss', {duration: 2000});
      }
    }));
  }

  constructor(private loginService: LoginService, private snackBarService: MatSnackBar) {
  }

}
