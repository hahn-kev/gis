import { Injectable } from '@angular/core';
import { LeaveRequestService } from '../leave-request.service';
import { LeaveRequestWithNames } from '../leave-request';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { LoginService } from '../../../services/auth/login.service';

@Injectable()
export class LeaveListResolverService implements Resolve<LeaveRequestWithNames[]> {
  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<LeaveRequestWithNames[]> | Promise<LeaveRequestWithNames[]> | LeaveRequestWithNames[] {
    const personId = route.params['personId'];
    if (personId === 'mine' || state.url.endsWith('mine')) {
      return this.leaveService.listMyLeave();
    }
    if (personId === 'supervisor' || state.url.endsWith('supervisor')) {
      return this.leaveService.listForSupervisor();
    }

    const supervisorId = route.params['supervisorId'];
    if (supervisorId)
      return this.leaveService.listForSupervisor(supervisorId);
    if (personId) {
      return this.leaveService.listByPersonId(personId);
    }
    return this.leaveService.list();
  }

  constructor(private leaveService: LeaveRequestService, private loginService: LoginService) {
  }

}
