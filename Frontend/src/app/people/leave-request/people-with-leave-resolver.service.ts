import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { PersonAndLeaveDetails } from './person-and-leave-details';
import { Observable } from 'rxjs';
import { LeaveRequestService } from './leave-request.service';
import { LoginService } from '../../services/auth/login.service';
import { first, switchMap } from 'rxjs/operators';

@Injectable()
export class PeopleWithLeaveResolverService implements Resolve<PersonAndLeaveDetails[]> {
  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<PersonAndLeaveDetails[]> | Promise<PersonAndLeaveDetails[]> | PersonAndLeaveDetails[] {
    return this.leaveRequestService.listPeopleWithLeave();
  }

  constructor(private leaveRequestService: LeaveRequestService) {
  }

}
