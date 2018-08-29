import {Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, Resolve, RouterStateSnapshot} from '@angular/router';
import {PersonAndLeaveDetails} from './person-and-leave-details';
import {Observable} from 'rxjs';
import {LeaveRequestService} from './leave-request.service';
import {Year} from "../training-requirement/year";

@Injectable()
export class PeopleWithLeaveResolverService implements Resolve<PersonAndLeaveDetails[]> {
  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<PersonAndLeaveDetails[]> | Promise<PersonAndLeaveDetails[]> | PersonAndLeaveDetails[] {
    let year = route.params['year'];
    if (!year) year = Year.CurrentSchoolYear();
    if (route.url.some(segment => segment.path == 'mine')) {
      return this.leaveRequestService.listMyPeopleWithLeave(year);
    }
    return this.leaveRequestService.listPeopleWithLeave(year);
  }

  constructor(private leaveRequestService: LeaveRequestService) {
  }

}
