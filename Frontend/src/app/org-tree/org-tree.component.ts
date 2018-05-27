import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { combineLatest } from 'rxjs';
import { Job, JobStatus } from '../job/job';
import { RoleExtended } from '../people/role';
import { OrgGroupWithSupervisor } from '../people/groups/org-group';
import { GroupOrgNode, JobOrgNode, OrgNode, RoleOrgNode } from './org-node';
import { NestedTreeControl } from '@angular/cdk/tree';
import { Observable } from 'rxjs/Rx';
import { UrlBindingService } from '../services/url-binding.service';
import { map } from 'rxjs/operators';
import { OrgTreeData } from './org-tree-data';


@Component({
  selector: 'app-org-tree',
  templateUrl: './org-tree.component.html',
  styleUrls: ['./org-tree.component.scss'],
  providers: [UrlBindingService]
})
export class OrgTreeComponent implements OnInit {
  treeControl: NestedTreeControl<OrgNode>;
  nodes: OrgNode[];
  data: OrgTreeData;
  rootId: string;

  constructor(private route: ActivatedRoute,
              public urlBinding: UrlBindingService<{ allRoles: boolean, allJobs: boolean, show: string[] }>) {
    this.urlBinding.addParam('allRoles', false);
    this.urlBinding.addParam('allJobs', false);
    this.urlBinding.addParam('show', ['staff']);
    this.treeControl = new NestedTreeControl<OrgNode>(dataNode => dataNode.observableChildren);
    this.treeControl.getLevel = dataNode => dataNode.level;
    combineLatest(this.route.params, this.route.data).subscribe(([params, data]: [
      {
        rootId: string
      },
      { treeData: OrgTreeData }
      ]) => {
      this.data = data.treeData;
      this.rootId = params.rootId;
      this.urlBinding.loadFromParams();
      this.buildList();
    });
  }

  buildList() {
    this.nodes = this.data.groups.filter(org => org.parentId == null || org.id == this.rootId)
      .map(org => this.buildOrgNode(org, this.data));
    for (let node of this.nodes) {
      this.treeControl.expand(node);
    }
  }

  buildJobNode(job: Job, data: OrgTreeData) {
    return new JobOrgNode(job.id,
      job,
      'job',
      data.roles
        .filter(role => role.jobId == job.id)
        .map(role => this.buildRoleNode(role, data)),
      combineLatest(this.urlBinding.observableValues.allRoles)
        .pipe(map(([allRoles]: [boolean]) => (value: RoleExtended) => value.active || allRoles))
    );
  }

  buildRoleNode(role: RoleExtended, data: OrgTreeData) {
    return new RoleOrgNode(role.personId, role, 'role', [], Observable.of(() => true));
  }


  buildOrgNode(org: OrgGroupWithSupervisor, data: OrgTreeData) {
    return new GroupOrgNode(org.id, org, 'org', [
        ...data.jobs
          .filter(value => value.orgGroupId == org.id && (this.urlBinding.values.allJobs || value.current))
          .map(value => this.buildJobNode(value, data)),

        ...data.groups
          .filter(value => value.parentId == org.id)
          .map(value => this.buildOrgNode(value, data))
      ],
      combineLatest(this.urlBinding.observableValues.allJobs, this.urlBinding.observableValues.show)
        .pipe(map(([allJobs, show]: [boolean, string[]]) =>
          (child: OrgGroupWithSupervisor | Job) => {
            if ('title' in child) {
              if (!allJobs && !child.current) return false;
              if (child.status == JobStatus.SchoolAid && !show.includes('aids')) return false;
              if (child.status != JobStatus.SchoolAid && !show.includes('staff')) return false;
            }
            return true;
          }))
    );
  }

  isOrgNode = (i: number, node: OrgNode) => {
    return node.type == 'org';
  };

  isJobNode = (i: number, node: OrgNode) => {
    return node.type == 'job';
  };

  isRoleNode = (i: number, node: OrgNode) => {
    return node.type == 'role';
  };

  trackBy(i: number, node: OrgNode) {
    return node.id;
  }

  findNode(nodes: OrgNode[], id: string): OrgNode {
    for (let node of nodes) {
      if (node.id == id) return node;
      let foundChild = this.findNode(node.allChildren, id);
      if (foundChild) return foundChild;
    }
    return null;
  }

  ngOnInit() {
  }

  dividerMargin(node: OrgNode) {
    return 48 + node.level * 40 + 'px';
  }

  isLast(node: OrgNode) {
    if (node.parent) {
      return node.parent.filteredChildren.findIndex(
        value => value.id == node.id) + 1 == node.parent.filteredChildren.length;
    }
    return this.nodes.findIndex(value => value.id == node.id) + 1 == this.nodes.length;
  }

  isLastVisible(node: OrgNode) {
    let visitedNodes = [node];
    if (node.hasChildren && this.treeControl.isExpanded(node)) return false;
    if (node.parent) {
      while (node.parent) {
        if (this.isLast(node)) {
          node = node.parent;
          if (visitedNodes.indexOf(node) != -1) throw new Error('Error loop in tree found');
          visitedNodes.push(node);
        } else {
          return false;
        }
      }
    }
    return this.isLast(node);
  }

}
