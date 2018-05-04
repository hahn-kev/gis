import { Injectable } from '@angular/core';
import { LeaveRequestService } from '../leave-request.service';
import { LeaveRequestWithNames } from '../leave-request';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { LoginService } from '../../../services/auth/login.service';
import { first, switchMap } from 'rxjs/operators';

@Injectable()
export class LeaveListResolverService implements Resolve<LeaveRequestWithNames[]> {
  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<LeaveRequestWithNames[]> | Promise<LeaveRequestWithNames[]> | LeaveRequestWithNames[] {
    const personId = route.params['personId'];
    if (personId === 'mine') {
      return this.loginService.currentUserToken()
        .pipe(first(),
          switchMap(userToken => this.leaveService.listByPersonId(userToken.personId)));
    }
    if (personId) {
      return this.leaveService.listByPersonId(personId);
    }
    return this.leaveService.list();
  }

  constructor(private leaveService: LeaveRequestService, private loginService: LoginService) {
  }

}
