import { Component, OnInit } from '@angular/core';
import { OrgGroup } from '../org-group';
import { ActivatedRoute, Router } from '@angular/router';
import { GroupService } from '../group.service';
import { PersonWithStaff } from '../../person';
import { OrgChain } from '../org-chain';
import { MatDialog, MatSnackBar } from '@angular/material';
import { ConfirmDialogComponent } from '../../../dialog/confirm-dialog/confirm-dialog.component';
import { BaseEditComponent } from '../../../components/base-edit-component';
import { Location } from '@angular/common';

@Component({
  selector: 'app-group',
  templateUrl: './group.component.html',
  styleUrls: ['./group.component.scss']
})
export class GroupComponent extends BaseEditComponent implements OnInit {
  public group: OrgGroup;
  public people: PersonWithStaff[];
  public groups: OrgGroup[];
  public children: OrgGroup[];
  public orgChain: OrgChain;

  public isNew = false;

  constructor(private route: ActivatedRoute,
              private router: Router,
              private groupService: GroupService,
              private snackBar: MatSnackBar,
              private location: Location,
              dialog: MatDialog) {
    super(dialog);
  }

  ngOnInit(): void {
    this.route.data.subscribe((value) => {
      this.group = value.group;
      this.isNew = !this.group.id;
      this.groups = value.groups;
      this.people = value.staff;
      this.children = this.isNew ? [] : this.groups.filter(group => group.parentId === this.group.id);
      setTimeout(() => this.refreshOrgChain());
    });
  }

  supervisorChanged() {
    let supervisor = this.people.find(p => p.id == this.group.supervisor);
    if (supervisor != null && supervisor.staff.email == null) {
      let editSuperSnackbar = this.snackBar.open(`Supervisor won't get leave requests, they don't have a staff email`,
        'Edit Supervisor',
        {duration: 20000});
      editSuperSnackbar.onAction().subscribe(value => {
        this.save(['people', 'edit', supervisor.id]);
      });
    }
    this.refreshOrgChain();
  }

  refreshOrgChain(): void {
    this.orgChain = this.groupService.buildOrgChain(this.group, this.people, this.groups);
  }

  async save(navigateTo = null): Promise<void> {
    await this.groupService.updateGroup(this.group);
    if (navigateTo) this.router.navigate(navigateTo);
    else this.location.back();
    this.snackBar.open(`${this.group.groupName} ${this.isNew ? 'Added' : 'Saved'}`,
      null,
      {duration: 2000});
  }

  async deleteGroup() {
    let result = await ConfirmDialogComponent.OpenWait(this.dialog,
      `Delete ${this.group.type} ${this.group.groupName}`,
      'Delete',
      'Cancel');
    if (!result) return;
    await this.groupService.deleteGroup(this.group.id);
    this.snackBar.open(`${this.group.type} ${this.group.groupName} deleted`, null, {duration: 2000});
    this.router.navigate(['/groups/list']);

  }
}
