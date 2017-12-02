import { Injectable } from '@angular/core';
import { OrgGroup } from '../org-group';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { GroupService } from '../group.service';

@Injectable()
export class GroupResolveService implements Resolve<OrgGroup> {
  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<OrgGroup> | Promise<OrgGroup> | OrgGroup {
    if (route.params['id'] === 'new') {
      return new OrgGroup();
    }
    return this.orgGroupService.getGroup(route.params['id']);
  }

  constructor(private orgGroupService: GroupService) {
  }

}
