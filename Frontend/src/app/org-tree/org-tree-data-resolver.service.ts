import { Injectable } from '@angular/core';
import { GroupService } from '../people/groups/group.service';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { OrgTreeData } from './org-tree-data';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class OrgTreeDataResolverService implements Resolve<OrgTreeData> {

  constructor(private orgGroupService: GroupService) {
  }

  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<OrgTreeData> | Promise<OrgTreeData> | OrgTreeData {
    return this.orgGroupService.getOrgTreeData(route.paramMap.get('rootId'));
  }
}
