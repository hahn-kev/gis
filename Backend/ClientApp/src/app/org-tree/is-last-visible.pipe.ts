import { Pipe, PipeTransform } from '@angular/core';
import { OrgNode } from './org-node';

@Pipe({
  name: 'isLastVisible'
})
export class IsLastVisiblePipe implements PipeTransform {

  transform(value: OrgNode, isExpanded: boolean, nodes: OrgNode[]): any {
    return this.isLastVisible(value, isExpanded, nodes);
  }

  isLastVisible(node: OrgNode, isExpanded: boolean, nodes: OrgNode[]) {
    console.log(`I'm called a bunch?`);
    if (node.hasChildren && isExpanded) return false;
    let visitedNodes = [node];
    if (node.parent) {
      while (node.parent) {
        if (this.isLast(node, nodes)) {
          node = node.parent;
          if (visitedNodes.indexOf(node) != -1) throw new Error('Error loop in tree found');
          visitedNodes.push(node);
        } else {
          return false;
        }
      }
    }
    return this.isLast(node, nodes);
  }

  isLast(node: OrgNode, nodes: OrgNode[]) {
    if (node.parent) {
      return node.parent.filteredChildren.findIndex(
        value => value.id == node.id) + 1 == node.parent.filteredChildren.length;
    }
    return nodes.findIndex(value => value.id == node.id) + 1 == nodes.length;
  }
}
