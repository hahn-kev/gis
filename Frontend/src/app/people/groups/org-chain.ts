import { OrgGroup } from './org-group';
import { Person } from '../person';

export class OrgChain {
  public linkStart: OrgChainLink;
  public linkEnd: OrgChainLink;

  constructor(public links: OrgChainLink[]) {
    this.linkStart = this.links[0];
    this.linkEnd = this.links[this.links.length - 1];
  }
}

export class OrgChainLink {
  public static FromGroup(orgGroup: OrgGroup): OrgChainLink {
    return new OrgChainLink(LinkType.Group, orgGroup.id, orgGroup.groupName);
  }

  public static FromPerson(person: Person): OrgChainLink {
    return new OrgChainLink(LinkType.Person, person.id, `${person.firstName} ${person.lastName}`);
  }

  constructor(public type: LinkType, public id: string, public title: string) {
  }
}

export enum LinkType {
  Group = 'groups',
  Person = 'people'
}
