import { Component, OnInit } from '@angular/core';
import { OrgGroup } from '../org-group';
import { ActivatedRoute, Router } from '@angular/router';
import { GroupService } from '../group.service';
import { Person } from '../../person';
import { OrgChain } from '../org-chain';
import { MatDialog, MatSnackBar } from '@angular/material';
import { ConfirmDialogComponent } from '../../../dialog/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-group',
  templateUrl: './group.component.html',
  styleUrls: ['./group.component.scss']
})
export class GroupComponent implements OnInit {
  public group: OrgGroup;
  public people: Person[];
  public groups: OrgGroup[];
  public children: OrgGroup[];
  public orgChain: OrgChain;

  public isNew = false;

  constructor(private route: ActivatedRoute,
              private router: Router,
              private groupService: GroupService,
              private snackBar: MatSnackBar,
              private dialog: MatDialog) {
  }

  ngOnInit(): void {
    this.route.data.subscribe((value) => {
      this.group = value.group;
      this.isNew = !this.group.id;
      this.groups = value.groups;
      this.people = value.people;
      this.children = this.isNew ? [] : this.groups.filter(group => group.parentId === this.group.id);
      setTimeout(() => this.refreshOrgChain());
    });
  }

  refreshOrgChain(): void {
    this.orgChain = this.groupService.buildOrgChain(this.group, this.people, this.groups);
  }

  async save(): Promise<void> {
    await this.groupService.updateGroup(this.group);
    this.router.navigate(['/groups/list']);
    this.snackBar.open(`${this.group.type} ${this.isNew ? 'Added' : 'Saved'}`, null, {duration: 2000});
  }

  async deleteGroup() {
    let result = await ConfirmDialogComponent.OpenWait(this.dialog, `Delete ${this.group.type} ${this.group.groupName}`, 'Delete', 'Cancel');
    if (!result) return;
    await this.groupService.deleteGroup(this.group.id);
    this.snackBar.open(`${this.group.type} ${this.group.groupName} deleted`, null, {duration: 2000});
    this.router.navigate(['/groups/list']);

  }
}
