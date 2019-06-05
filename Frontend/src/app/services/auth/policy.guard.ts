import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { PolicyService } from './policy.service';
import { LoginService } from './login.service';
import { map, tap } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable()
export class PolicyGuard implements CanActivate {


  constructor(private policyService: PolicyService,
              private loginService: LoginService,
              private snackBarService: MatSnackBar) {
  }

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    let policy = next.data.requirePolicy;
    if (!policy) {
      console.warn(`PolicyGuard used on ${next.url} without a policy being set, letting user through`);
      return true;
    }
    return this.loginService.currentUserToken()
      .pipe(
        map(value => this.policyService.getPolicy(policy)(value)),
        tap(allowed => {
          if (!allowed) {
            this.snackBarService.open(`Access denied, missing policy [${policy}]`, 'dismiss', {duration: 4000});
          }
        })
      );
  }
}
