import {Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, Resolve, RouterStateSnapshot} from '@angular/router';
import {PersonAndLeaveDetails} from './person-and-leave-details';
import {Observable} from 'rxjs';
import {LeaveRequestService} from './leave-request.service';

@Injectable()
export class PeopleWithLeaveResolverService implements Resolve<PersonAndLeaveDetails[]> {
  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<PersonAndLeaveDetails[]> | Promise<PersonAndLeaveDetails[]> | PersonAndLeaveDetails[] {
    if (state.url.endsWith('mine')) {
      return this.leaveRequestService.listMyPeopleWithLeave();
    }
    return this.leaveRequestService.listPeopleWithLeave();
  }

  constructor(private leaveRequestService: LeaveRequestService) {
  }

}
