import { RoleExtended } from '../people/role';
import { OrgGroupWithSupervisor } from '../people/groups/org-group';
import { Job } from '../job/job';

export class OrgNode<T> {
  id: string;
  value: T;
  type: 'job' | 'org' | 'role';
  children: OrgNode<RoleExtended | OrgGroupWithSupervisor | Job>[];

  constructor(id: string,
              value: T,
              type: 'job' | 'org' | 'role',
              children: OrgNode<RoleExtended | OrgGroupWithSupervisor | Job>[]) {
    this.id = id;
    this.value = value;
    this.type = type;
    this.children = children;
  }
}
