import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { combineLatest } from 'rxjs';
import { Job } from '../job/job';
import { RoleExtended } from '../people/role';
import { OrgGroupWithSupervisor } from '../people/groups/org-group';
import { OrgNode } from './org-node';
import { NestedTreeControl } from '@angular/cdk/tree';
import { Observable } from 'rxjs/Rx';
import { UrlBindingService } from '../services/url-binding.service';
import { map } from 'rxjs/operators';

interface Data {
  roles: RoleExtended[];
  jobs: Job[];
  groups: OrgGroupWithSupervisor[];
}

@Component({
  selector: 'app-org-tree',
  templateUrl: './org-tree.component.html',
  styleUrls: ['./org-tree.component.scss'],
  providers: [UrlBindingService]
})
export class OrgTreeComponent implements OnInit {
  nestedTreeControl: NestedTreeControl<OrgNode<any>>;
  nodes: OrgNode<any>[];
  data: Data;
  rootId: string;

  constructor(private route: ActivatedRoute,
              public urlBinding: UrlBindingService<{ allRoles: boolean, allJobs: boolean }>) {
    this.urlBinding.addParam('allRoles', true);
    this.urlBinding.addParam('allJobs', true);
    this.nestedTreeControl = new NestedTreeControl<OrgNode<any>>(dataNode => dataNode.observableChildren);
    combineLatest(this.route.params, this.route.data).subscribe(([params, data]: [
      {
        rootId: string
      }, Data
      ]) => {
      this.data = data;
      this.rootId = params.rootId;
      if (!this.urlBinding.loadFromParams()) this.buildList();
    });
  }

  buildList() {
    this.nodes = this.data.groups.filter(org => org.parentId == null)
      .map(org => this.buildOrgNode(org, this.data));
    if (this.rootId) {
      this.nodes = [this.findNode(this.nodes, this.rootId)].filter(value => value != null);
    }
    this.nestedTreeControl.dataNodes = this.nodes;
    for (let node of this.nodes) {
      this.nestedTreeControl.expand(node);
    }
  }

  buildJobNode(job: Job, data: Data) {
    return new OrgNode(job.id,
      job,
      'job',
      data.roles
        .filter(role => role.jobId == job.id)
        .map(role => this.buildRoleNode(role, data)),
      combineLatest(this.urlBinding.observableValues.allRoles)
        .pipe(map(([allRoles]: [boolean]) => (value: RoleExtended) => value.active || allRoles))
    );
  }

  buildRoleNode(role: RoleExtended, data: Data) {
    return new OrgNode(role.personId, role, 'role', [], Observable.of(() => true));
  }


  buildOrgNode(org: OrgGroupWithSupervisor, data: Data) {
    return new OrgNode(org.id, org, 'org', [
        ...data.jobs
          .filter(value => value.orgGroupId == org.id && (this.urlBinding.values.allJobs || value.current))
          .map(value => this.buildJobNode(value, data)),

        ...data.groups
          .filter(value => value.parentId == org.id)
          .map(value => this.buildOrgNode(value, data))
      ],
      combineLatest(this.urlBinding.observableValues.allJobs)
        .pipe(map(([allJobs]: [boolean]) =>
          (child: OrgGroupWithSupervisor | Job) => {
            if ('title' in child) {
              return allJobs || child.current;
            } else {
              return true;
            }
          }))
    );
  }

  isOrgNode = (i: number, node: OrgNode<any>) => {
    return node.type == 'org';
  };

  isJobNode = (i: number, node: OrgNode<any>) => {
    return node.type == 'job';
  };

  isRoleNode = (i: number, node: OrgNode<any>) => {
    return node.type == 'role';
  };

  isOrg(node: OrgNode<any>): node is OrgNode<OrgGroupWithSupervisor> {
    return node.type == 'org';
  }

  isJob(node: OrgNode<any>): node is OrgNode<Job, RoleExtended> {
    return node.type == 'job';
  }

  isRole(node: OrgNode<any>): node is OrgNode<RoleExtended> {
    return node.type == 'role';
  }

  trackBy(i: number, node: OrgNode<any>) {
    return node.id;
  }

  filledJobs(node: OrgNode<any>) {
    if (this.isJob(node)) {
      return node.filteredChildren.filter(child => child.value.active).length;
    } else if (this.isOrg(node)) {
      return node.filteredChildren.reduce((previousValue,
                                           currentValue) => previousValue + this.filledJobs(currentValue), 0);
    }
    return 0;
  }

  jobsAvalible(node: OrgNode<any>) {
    if (this.isJob(node)) {
      return node.value.positions;
    } else if (this.isOrg(node)) {
      return node.filteredChildren
        .reduce((previousValue, currentValue) => previousValue + this.jobsAvalible(currentValue), 0);
    }
    return 0;
  }

  openJobs(node: OrgNode<any>) {
    if (this.isJob(node)) {
      return node.value.positions - node.filteredChildren.filter(child => child.value.active).length;
    } else if (this.isOrg(node)) {
      return node.filteredChildren.reduce((previousValue,
                                           currentValue) => previousValue + this.filledJobs(currentValue), 0);
    }
    return 0;
  }

  findNode(nodes: OrgNode<any>[], id: string): OrgNode<any> {
    for (let node of nodes) {
      if (node.id == id) return node;
      let foundChild = this.findNode(node.allChildren, id);
      if (foundChild) return foundChild;
    }
    return null;
  }

  ngOnInit() {
  }

}
