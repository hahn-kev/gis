import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { PersonAndLeaveDetails } from './person-and-leave-details';
import { Observable } from 'rxjs';
import { LeaveRequestService } from './leave-request.service';
import { Year } from '../training-requirement/year';
import { PolicyService } from '../../services/auth/policy.service';

@Injectable()
export class PeopleWithLeaveResolverService implements Resolve<PersonAndLeaveDetails[]> {
  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<PersonAndLeaveDetails[]> | Promise<PersonAndLeaveDetails[]> | PersonAndLeaveDetails[] {
    let year = route.params['year'];
    if (!year) year = Year.CurrentSchoolYear();
    if (route.data.all) {
      return this.leaveRequestService.listPeopleWithLeave(year);
    }
    if (route.data.supervisor) {
      //this should come before the policy tests
      return this.leaveRequestService.listMyPeopleWithLeave(year);
    }
    if (this.policyService.testPolicy('leaveManager')) {
      return this.leaveRequestService.listPeopleWithLeave(year);
    }
    if (this.policyService.testPolicy('leaveSupervisor')) {
      return this.leaveRequestService.listMyPeopleWithLeave(year);
    }
    return this.leaveRequestService.listMyLeaveDetails(year);
  }

  constructor(private leaveRequestService: LeaveRequestService, private policyService: PolicyService) {
  }

}
