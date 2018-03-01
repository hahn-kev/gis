import { Component, OnInit } from '@angular/core';
import { OrgGroup } from '../org-group';
import { ActivatedRoute, Router } from '@angular/router';
import { GroupService } from '../group.service';
import { Person } from '../../person';
import { OrgChain } from '../org-chain';
import { MatSnackBar } from '@angular/material';

@Component({
  selector: 'app-group',
  templateUrl: './group.component.html',
  styleUrls: ['./group.component.scss']
})
export class GroupComponent implements OnInit {
  public group: OrgGroup;
  public people: Person[];
  public groups: OrgGroup[];
  public orgChain: OrgChain;
  private isNew = false;

  constructor(private route: ActivatedRoute,
              private router: Router,
              private groupService: GroupService,
              private snackBar: MatSnackBar) {
  }

  ngOnInit(): void {
    this.route.data.subscribe((value) => {
      this.group = value.group;
      this.isNew = !this.group.id;
      this.groups = value.groups;
      this.people = value.people;
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
}
