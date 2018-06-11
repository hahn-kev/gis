import { OrgGroupWithSupervisor } from '../people/groups/org-group';
import { RoleExtended } from '../people/role';
import { Job } from '../job/job';

export class OrgTreeData {
  roles: RoleExtended[];
  jobs: Job[];
  groups: OrgGroupWithSupervisor[];
}
