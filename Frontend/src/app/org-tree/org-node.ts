import { RoleExtended } from '../people/role';
import { OrgGroup, OrgGroupWithSupervisor } from '../people/groups/org-group';
import { Job, JobStatus } from '../job/job';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export class OrgNode<T = RoleExtended | OrgGroupWithSupervisor | Job,
  C = RoleExtended | OrgGroupWithSupervisor | Job,
  P = RoleExtended | OrgGroupWithSupervisor | Job> {
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
              observableFilter: Observable<(value: C) => boolean>) {
    this.id = id;
    this.value = value;
    this.type = type;
    this.allChildren = children;
    for (let child of this.allChildren) {
      child.parent = this;
    }
    this.observableChildren = new BehaviorSubject(this.allChildren);
    observableFilter.pipe(map(filter => this.allChildren.filter(child => filter(child.value))))
      .subscribe(this.observableChildren);
  }
}

export class JobOrgNode extends OrgNode<Job, RoleExtended, OrgGroupWithSupervisor> {

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

export class GroupOrgNode extends OrgNode<OrgGroupWithSupervisor, Job | OrgGroupWithSupervisor, OrgGroup> {

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
