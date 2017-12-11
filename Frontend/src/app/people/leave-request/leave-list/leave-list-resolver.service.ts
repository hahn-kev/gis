import { Injectable } from '@angular/core';
import { LeaveRequestService } from '../leave-request.service';
import { LeaveRequestWithNames } from '../leave-request';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class LeaveListResolverService implements Resolve<LeaveRequestWithNames[]> {
  resolve(route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<LeaveRequestWithNames[]> | Promise<LeaveRequestWithNames[]> | LeaveRequestWithNames[] {
    return this.leaveService.list();
  }

  constructor(private leaveService: LeaveRequestService) {
  }

}
