import {Injectable} from '@angular/core';
import {LeaveRequestService} from "./leave-request.service";
import {ActivatedRouteSnapshot, Resolve, RouterStateSnapshot} from "@angular/router";
import {Observable} from "rxjs/Observable";
import {LeaveRequestWithNames} from "./leave-request";

@Injectable()
export class LeaveRequestSupervisorResolverService implements Resolve<LeaveRequestWithNames[]> {

  constructor(private leaveService: LeaveRequestService) {
  }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<LeaveRequestWithNames[]> | Promise<LeaveRequestWithNames[]> | LeaveRequestWithNames[] {
    return this.leaveService.listForSupervisor(route.params['supervisorId']);
  }
}
