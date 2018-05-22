import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { OrgGroup, OrgGroupWithSupervisor } from './org-group';
import { Observable } from 'rxjs';
import { Person } from '../person';
import { LinkType, OrgChain, OrgChainLink } from './org-chain';
import { map } from 'rxjs/internal/operators';

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

  buildOrgChain(orgGroup: OrgGroup, people: Person[], groups: OrgGroup[]): OrgChain {
    let chainList: OrgChainLink[] = [];
    let currentGroup = orgGroup;
    while (currentGroup != null) {
      chainList.push(OrgChainLink.FromGroup(currentGroup));
      if (currentGroup.supervisor != null && currentGroup != orgGroup) break;
      currentGroup = groups.find(value => currentGroup.parentId == value.id);
      if (currentGroup == null) break;
      if (currentGroup.id == orgGroup.id) currentGroup = orgGroup;
      if (chainList.length > 1 && currentGroup == orgGroup) throw new Error('Circular Orginization chart detected, please resolve');
    }
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

  isChildOf(childOrgId: string, parentOrgId: string, orgGroups: OrgGroup[] | Map<string, OrgGroup>) {
    if (childOrgId == parentOrgId) return true;
    if (orgGroups instanceof Array) {
      orgGroups = new Map<string, OrgGroup>(orgGroups.map((group): [string, OrgGroup] => [group.id, group]));
    }
    let map = (<Map<string, OrgGroup>>orgGroups);
    let groupId = childOrgId;
    let group: OrgGroup;

    while (true) {
      group = map.get(groupId);
      if (group == undefined) return false;
      if (group.parentId == parentOrgId) return true;
      groupId = group.parentId;
      if (groupId == childOrgId) throw new Error('Circular Orginization chart detected, please resolve');
    }
  }
}
