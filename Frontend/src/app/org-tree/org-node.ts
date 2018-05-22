import { RoleExtended } from '../people/role';
import { OrgGroupWithSupervisor } from '../people/groups/org-group';
import { Job } from '../job/job';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Observable } from 'rxjs/Observable';
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
