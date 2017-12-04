import { Component, OnInit } from '@angular/core';
import { OrgGroup } from '../org-group';
import { ActivatedRoute, Router } from '@angular/router';
import { GroupService } from '../group.service';
import { Person } from '../../person';

@Component({
  selector: 'app-group',
  templateUrl: './group.component.html',
  styleUrls: ['./group.component.scss']
})
export class GroupComponent implements OnInit {
  public group: OrgGroup;
  public people: Person[];
  public groups: OrgGroup[];

  constructor(private route: ActivatedRoute,
              private router: Router,
              private groupService: GroupService) {
  }

  ngOnInit(): void {
    this.route.data.subscribe((value) => {
      this.group = value.group;
      this.groups = value.groups;
      this.people = value.people;
    });
  }

  async save(): Promise<void> {
    await this.groupService.updateGroup(this.group);
    this.router.navigate(['/groups']);
  }
}
