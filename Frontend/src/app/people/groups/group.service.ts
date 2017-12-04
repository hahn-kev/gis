import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { OrgGroup } from './org-group';
import { Observable } from 'rxjs/Observable';
import { Person } from '../person';
import { LinkType, OrgChain, OrgChainLink } from './org-chain';

@Injectable()
export class GroupService {

  constructor(private http: HttpClient) {
  }

  getGroup(id: string): Observable<OrgGroup> {
    return this.http.get<OrgGroup>('/api/orggroup/' + id);
  }

  getAll(): Observable<OrgGroup[]> {
    return this.http.get<OrgGroup[]>('/api/orggroup');
  }

  updateGroup(group: OrgGroup): Promise<OrgGroup> {
    return this.http.post('/api/orggroup', group).toPromise();
  }

  buildOrgChain(orgGroup: OrgGroup, people: Person[], groups: OrgGroup[]): OrgChain {
    let chainList = [];
    let currentGroup = orgGroup;
    while (currentGroup != null) {
      chainList.push(OrgChainLink.FromGroup(currentGroup));
      if (currentGroup.supervisor != null && currentGroup != orgGroup) break;
      currentGroup = groups.find(value => currentGroup.parentId == value.id);
    }
    if (currentGroup != null && currentGroup.supervisor != null) {
      chainList.push(OrgChainLink.FromPerson(people.find(value => currentGroup.supervisor == value.id)));
    } else {
      chainList.push(new OrgChainLink(LinkType.Group, '', 'No One'));
    }
    return new OrgChain(chainList);
  }
}
