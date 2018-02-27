import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { PersonAndLeaveDetails } from './person-and-leave-details';
import { Observable } from 'rxjs/Observable';
import { LeaveRequestService } from './leave-request.service';
import { LoginService } from '../../services/auth/login.service';
import { first, switchMap, tap } from 'rxjs/operators';

@Injectable()
export class PeopleWithLeaveResolverService implements Resolve<PersonAndLeaveDetails[]> {
  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<PersonAndLeaveDetails[]> | Promise<PersonAndLeaveDetails[]> | PersonAndLeaveDetails[] {
    return this.loginService.hasAnyRole(['admin', 'hr']).pipe(
      switchMap(isHrAdmin => this.leaveRequestService.listPeopleWithLeave(isHrAdmin)),
      first(),
      tap(x => {
        },
        e => {
        },
        () => console.log('done'))
    );
    //todo complete isn't always called, no idea why documentation website is down... help!
  }

  constructor(private leaveRequestService: LeaveRequestService, private loginService: LoginService) {
  }

}
