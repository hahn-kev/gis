import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { OrgGroup } from '../org-group';
import { Observable } from 'rxjs/Observable';
import { GroupService } from '../group.service';

@Injectable()
export class GroupsResolveService implements Resolve<OrgGroup[]> {
  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<OrgGroup[]> | Promise<OrgGroup[]> | OrgGroup[] {
    return this.orgGroupService.getAll();
  }

  constructor(private orgGroupService: GroupService) {
  }

}
