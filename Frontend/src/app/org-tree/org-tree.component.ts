import {Component, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {combineLatest, Observable} from 'rxjs';
import {Job, JobStatus} from '../job/job';
import {RoleExtended} from '../people/role';
import {OrgGroupWithSupervisor} from '../people/groups/org-group';
import {GroupOrgNode, JobOrgNode, OrgNode, RoleOrgNode} from './org-node';
import {NestedTreeControl} from '@angular/cdk/tree';
import {UrlBindingService} from '../services/url-binding.service';
import {map} from 'rxjs/operators';
import {OrgTreeData} from './org-tree-data';


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
              public urlBinding: UrlBindingService<{ allRoles: boolean, allJobs: boolean, show: string[], expanded: string[] }>) {
    this.urlBinding.addParam('allRoles', false);
    this.urlBinding.addParam('allJobs', false);
    this.urlBinding.addParam('show', ['staff']);
    this.urlBinding.addParam('expanded', []);
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
    let expanded = this.urlBinding.values.expanded;
    if (expanded.length == 0) {
      for (let node of this.nodes) {
        this.treeControl.expand(node);
        this.nodeToggled(node);
      }
    } else {
      for (let nodeIdToExpand of expanded) {
        let node = this.findNode(this.nodes, nodeIdToExpand);
        if (node) this.treeControl.expand(node);
      }
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

  trackNodesBy(i: number, node: OrgNode) {
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


  nodeToggled(node: OrgNode) {
    if (!node.hasChildren) return;
    let expanded = this.urlBinding.values.expanded.filter(value => value != node.id);
    if (this.treeControl.isExpanded(node)) {
      expanded.push(node.id);
    }
    this.urlBinding.values.expanded = expanded;
  }

}
