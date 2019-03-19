import { RoleExtended } from '../people/role';
import { OrgGroup, OrgGroupWithSupervisor } from '../people/groups/org-group';
import { Job, JobStatus } from '../job/job';
import { BehaviorSubject, combineLatest, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { UrlBindingService } from '../services/url-binding.service';

type TreeOrgGroupType = OrgGroupWithSupervisor;

export class OrgTree {
  data: OrgTreeData;
  urlBinding: UrlBindingService<{ allRoles: boolean, allJobs: boolean, show: string[] }>;
  nodes: OrgNode[];

  supervisorCountById: { [supervisorId: string]: number } = {};

  constructor(data: OrgTreeData,
              urlBinding?: UrlBindingService<{ allRoles: boolean; allJobs: boolean; show: string[] }>,
              rootId?: string) {
    this.data = data;
    this.urlBinding = urlBinding;
    this.nodes = data.groups.filter(org => org.parentId == null || org.id == rootId)
      .map(org => this.buildOrgNode(org, this.data));
    for (let group of data.groups) {
      if (!group.supervisor) continue;
      this.supervisorCountById[group.supervisor] = (this.supervisorCountById[group.supervisor] || 0) + 1;
    }
  }

  buildJobNode(job: Job, data: OrgTreeData) {
    return new JobOrgNode(job.id,
      job,
      'job',
      data.roles
        .filter(role => role.jobId == job.id)
        .map(role => this.buildRoleNode(role, data)),
      this.jobFilter()
    );
  }

  jobFilter() {
    if (!this.urlBinding) return undefined;
    return combineLatest(this.urlBinding.observableValues.allRoles)
      .pipe(map(([allRoles]: [boolean]) => (value: RoleExtended) => value.active || allRoles));
  }

  buildRoleNode(role: RoleExtended, data: OrgTreeData) {
    return new RoleOrgNode(role.personId, role, 'role', []);
  }

  buildOrgNode(org: TreeOrgGroupType, data: OrgTreeData) {
    return new GroupOrgNode(org.id, org, 'org', [
        ...data.jobs
          .filter(value => value.orgGroupId == org.id)
          .map(value => this.buildJobNode(value, data)),

        ...data.groups
          .filter(value => value.parentId == org.id)
          .map(value => this.buildOrgNode(value, data))
      ],
      this.orgFilter()
    );
  }

  orgFilter() {
    if (!this.urlBinding) return undefined;
    return combineLatest(this.urlBinding.observableValues.allJobs, this.urlBinding.observableValues.show)
      .pipe(map(([allJobs, show]: [boolean, string[]]) =>
        (child: TreeOrgGroupType | Job) => {
          if ('title' in child) {
            if (!allJobs && !child.current) return false;
            if (child.status == JobStatus.SchoolAid && !show.includes('aids')) return false;
            if (child.status != JobStatus.SchoolAid && !show.includes('staff')) return false;
          }
          return true;
        }));
  }
}

export class OrgTreeData {
  roles: RoleExtended[];
  jobs: Job[];
  groups: TreeOrgGroupType[];
}


export class OrgNode<T = RoleExtended | TreeOrgGroupType | Job,
  C = RoleExtended | TreeOrgGroupType | Job,
  P = RoleExtended | TreeOrgGroupType | Job> {
  id: string;
  value: T;
  type: 'job' | 'org' | 'role';
  parent: OrgNode<P, any, any> = null;
  allChildren: OrgNode<C, any, any>[];

  get hasChildren() {
    return this.filteredChildren.length > 0;
  }

  get filteredChildren(): OrgNode<C>[] {
    return this.observableChildren.value;
  }

  get level(): number {
    return !this.parent ? 0 : (this.parent.level + 1);
  }

  get openJobs(): number {
    return 0;
  }

  get jobsAvalible(): number {
    return 0;
  }

  get activeStaff(): number {
    return 0;
  }

  get activeAids(): number {
    return 0;
  }

  get allDecendantsGroups(): string[] {
    return [];
  }

  observableChildren: BehaviorSubject<OrgNode<C>[]>;


  constructor(id: string,
              value: T,
              type: 'job' | 'org' | 'role',
              children: OrgNode<C>[],
              observableFilter?: Observable<(value: C) => boolean>) {
    this.id = id;
    this.value = value;
    this.type = type;
    this.allChildren = children;
    for (let child of this.allChildren) {
      child.parent = this;
    }
    this.observableChildren = new BehaviorSubject(this.allChildren);
    if (observableFilter)
      observableFilter.pipe(map(filter => this.allChildren.filter(child => filter(child.value))))
        .subscribe(this.observableChildren);
  }
}

export class JobOrgNode extends OrgNode<Job, RoleExtended, TreeOrgGroupType> {

  get openJobs(): number {
    return this.jobsAvalible - (this.filteredChildren.filter(child => child.value.active).length);
  }

  get jobsAvalible(): number {
    return this.value.positions;
  }

  get activeStaff(): number {
    if (this.value.status == JobStatus.SchoolAid) return 0;
    return (this.filteredChildren.filter(child => child.value.active).length);
  }

  get activeAids(): number {
    if (this.value.status != JobStatus.SchoolAid) return 0;
    return (this.filteredChildren.filter(child => child.value.active).length);
  }
}

export class GroupOrgNode extends OrgNode<TreeOrgGroupType, Job | TreeOrgGroupType, OrgGroup> {

  get openJobs(): number {
    return this.filteredChildren.reduce((previousValue, currentValue) => previousValue + currentValue.openJobs, 0);
  }

  get jobsAvalible(): number {
    return this.filteredChildren.reduce((previousValue, currentValue) => previousValue + currentValue.jobsAvalible, 0);
  }

  get activeStaff(): number {
    return this.filteredChildren.reduce((previousValue, currentValue) => previousValue + currentValue.activeStaff, 0);
  }

  get activeAids(): number {
    return this.filteredChildren.reduce((previousValue, currentValue) => previousValue + currentValue.activeAids, 0);
  }

  _allDecendantsGroups: string[] = null;
  get allDecendantsGroups(): string[] {
    if (this._allDecendantsGroups == null) {
      this._allDecendantsGroups = [this.value.groupName].concat(...this.allChildren.map(
        value => value.allDecendantsGroups));
    }
    return this._allDecendantsGroups;
  }
}

export class RoleOrgNode extends OrgNode<RoleExtended, never, Job> {

}
