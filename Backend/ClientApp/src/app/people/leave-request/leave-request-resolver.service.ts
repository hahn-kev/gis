import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { LeaveRequestWithNames } from './leave-request';
import { LeaveRequestService } from './leave-request.service';
import { Observable } from 'rxjs';

@Injectable()
export class LeaveRequestResolverService implements Resolve<LeaveRequestWithNames> {
  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<LeaveRequestWithNames> | Promise<LeaveRequestWithNames> | LeaveRequestWithNames {
    if (route.params['id'] === 'new') return new LeaveRequestWithNames();
    return this.leaveService.getById(route.params['id']);
  }

  constructor(private leaveService: LeaveRequestService) {
  }
}
