import { Injectable } from '@angular/core';
import { LeaveRequestService } from '../leave-request.service';
import { LeaveRequestWithNames } from '../leave-request';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

@Injectable()
export class LeaveListResolverService implements Resolve<LeaveRequestWithNames[]> {
  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<LeaveRequestWithNames[]> | Promise<LeaveRequestWithNames[]> | LeaveRequestWithNames[] {

    if (route.data.mine) {
      return this.leaveService.listMyLeave();
    }
    if (route.data.supervisor) {
      return this.leaveService.listForSupervisor();
    }
    if (route.data.all) {
      return this.leaveService.list();
    }

    const supervisorId = route.params['supervisorId'];
    const personId = route.params['personId'];
    if (supervisorId)
      return this.leaveService.listForSupervisor(supervisorId);
    if (personId) {
      return this.leaveService.listByPersonId(personId);
    }
    return this.leaveService.listMyLeave();
  }

  constructor(private leaveService: LeaveRequestService) {
  }

}
