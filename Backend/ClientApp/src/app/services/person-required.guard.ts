import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LoginService } from './auth/login.service';
import { map, tap } from 'rxjs/operators';

@Injectable()
export class PersonRequiredGuard implements CanActivate {

  constructor(private loginService: LoginService,
              private snackBarService: MatSnackBar) {
  }

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    return this.loginService.currentUserToken().pipe(
      tap(user => {
        if (!user || !user.personId) {
          this.snackBarService.open(
            'No person was found attached to your account.' +
            ` Your email is listed as ${user.email} it must match a staff email for you to complete this action`,
            'dismiss');
        }
      }),
      map(user => user && !!user.personId));
  }
}
