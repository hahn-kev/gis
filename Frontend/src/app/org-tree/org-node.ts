import { RoleExtended } from '../people/role';
import { OrgGroupWithSupervisor } from '../people/groups/org-group';
import { Job } from '../job/job';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Observable } from 'rxjs/Observable';
import { map } from 'rxjs/operators';

export class OrgNode<T, L = RoleExtended | OrgGroupWithSupervisor | Job> {
  id: string;
  value: T;
  type: 'job' | 'org' | 'role';
  allChildren: OrgNode<L>[];

  get hasChildren() {
    return this.filteredChildren.length > 0;
  }

  get filteredChildren(): OrgNode<L>[] {
    return this.observableChildren.value;
  }

  observableChildren: BehaviorSubject<OrgNode<L>[]>;


  constructor(id: string,
              value: T,
              type: 'job' | 'org' | 'role',
              children: OrgNode<L>[],
              observableFilter: Observable<(value: L) => boolean>) {
    this.id = id;
    this.value = value;
    this.type = type;
    this.allChildren = children;
    this.observableChildren = new BehaviorSubject(this.allChildren);
    observableFilter.pipe(map(filter => this.allChildren.filter(child => filter(child.value))))
      .subscribe(this.observableChildren);
  }
}
