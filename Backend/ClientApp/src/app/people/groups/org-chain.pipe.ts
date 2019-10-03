import { Pipe, PipeTransform } from '@angular/core';
import { GroupService } from './group.service';
import { OrgGroup } from './org-group';
import { Person } from '../person';
import { OrgChainLink } from './org-chain';

@Pipe({
  name: 'orgChain'
})
export class OrgChainPipe implements PipeTransform {


  constructor(private groupService: GroupService) {
  }

  transform(orgGroupId: string, people: Person[], groups: OrgGroup[], currentPersonId?: string): OrgChainLink[] {
    if (!orgGroupId) return [];
    if (groups == null) return [];
    let orgGroup = groups.find(value => value.id == orgGroupId);
    if (!orgGroup) return [];
    return this.groupService.buildOrgChain(orgGroup, people, groups, currentPersonId).links;
  }

}
