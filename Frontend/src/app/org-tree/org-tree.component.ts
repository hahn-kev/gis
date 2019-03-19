import { combineLatest } from 'rxjs';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { OrgNode, OrgTree, OrgTreeData } from './org-node';
import { NestedTreeControl } from '@angular/cdk/tree';
import { UrlBindingService } from '../services/url-binding.service';


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
  orgTree: OrgTree;
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
    this.orgTree = new OrgTree(this.data, this.urlBinding, this.rootId);
    this.nodes = this.orgTree.nodes;
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
