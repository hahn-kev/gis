import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { OrgGroup, OrgGroupWithSupervisor } from './org-group';
import { Observable } from 'rxjs';
import { Person } from '../person';
import { LinkType, OrgChain, OrgChainLink } from './org-chain';
import { map } from 'rxjs/internal/operators';
import { OrgTreeData } from '../../org-tree/org-node';

@Injectable()
export class GroupService {

  constructor(private http: HttpClient) {
  }

  getGroup(id: string): Observable<OrgGroup> {
    return this.http.get<OrgGroup>('/api/orggroup/' + id);
  }

  getAll(): Observable<OrgGroupWithSupervisor[]> {
    return this.http.get<OrgGroupWithSupervisor[]>('/api/orggroup');
  }

  getAllMap() {
    return this.getAll()
      .pipe(map(value => new Map<string, OrgGroup>(value.map((group): [string, OrgGroup] => [group.id, group]))));
  }

  updateGroup(group: OrgGroup): Promise<Object> {
    return this.http.post<OrgGroup>('/api/orggroup', group).toPromise();
  }

  deleteGroup(groupId: string) {
    return this.http.delete('/api/orggroup/' + groupId, {responseType: 'text'}).toPromise();
  }

  buildOrgChain(orgGroup: OrgGroup, people: Person[], groups: OrgGroup[], currentPersonId?: string): OrgChain {
    if (!orgGroup) return new OrgChain([]);
    if (!currentPersonId) currentPersonId = 'invalid_id';
    let chainList: OrgChainLink[] = [];
    let currentGroup = orgGroup;

    do {
      chainList.push(OrgChainLink.FromGroup(currentGroup));
      if (currentGroup.supervisor != null && currentGroup.approverIsSupervisor && currentGroup.supervisor != currentPersonId) break;
      this.checkForCircularChart(chainList);
      currentGroup = this.findParent(currentGroup, groups);
    } while (currentGroup != null);


    let supervisor = null;
    if (currentGroup != null && currentGroup.supervisor != null)
      supervisor = people.find(value => currentGroup.supervisor == value.id);
    if (supervisor != null) {
      chainList.push(OrgChainLink.FromPerson(supervisor));
    } else {
      chainList.push(new OrgChainLink(LinkType.Group, '', 'No One'));
    }
    return new OrgChain(chainList);
  }

  checkForCircularChart(chainList: OrgChainLink[]) {
    let chainWithoutDuplicates = chainList.map(value => value.id)
      .filter((value, index, array) => array.indexOf(value) == index && value != null);

    if (chainList.length != chainWithoutDuplicates.length)
      throw new Error('Circular Orginization chart detected, please resolve');
  }

  findParent(child: OrgGroup, orgs: OrgGroup[]) {
    if (!child.parentId) return null;
    return orgs.find(value => child.parentId == value.id);
  }

  isChildOf(childOrgId: string, parentOrgId: string, orgGroups: OrgGroup[] | Map<string, OrgGroup>) {
    if (childOrgId == parentOrgId) return true;
    if (orgGroups instanceof Array) {
      orgGroups = new Map<string, OrgGroup>(orgGroups.map((g): [string, OrgGroup] => [g.id, g]));
    }
    let groupId = childOrgId;
    let group: OrgGroup;

    while (true) {
      group = orgGroups.get(groupId);
      if (group == undefined) return false;
      if (group.parentId == parentOrgId) return true;
      groupId = group.parentId;
      if (groupId == childOrgId) throw new Error('Circular Orginization chart detected, please resolve');
    }
  }

  getOrgTreeData(rootId: string) {
    return this.http.get<OrgTreeData>('/api/orgGroup/orgTreeData/' + (rootId || ''));
  }
}
