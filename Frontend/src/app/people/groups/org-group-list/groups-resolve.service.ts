import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { OrgGroupWithSupervisor } from '../org-group';
import { Observable } from 'rxjs';
import { GroupService } from '../group.service';

@Injectable()
export class GroupsResolveService implements Resolve<OrgGroupWithSupervisor[]> {
  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<OrgGroupWithSupervisor[]> | Promise<OrgGroupWithSupervisor[]> | OrgGroupWithSupervisor[] {
    return this.orgGroupService.getAll();
  }

  constructor(private orgGroupService: GroupService) {
  }

}
