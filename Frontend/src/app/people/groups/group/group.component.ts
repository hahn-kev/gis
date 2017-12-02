import { Component, OnInit } from '@angular/core';
import { OrgGroup } from '../org-group';
import { ActivatedRoute, Router } from '@angular/router';
import { GroupService } from '../group.service';

@Component({
  selector: 'app-group',
  templateUrl: './group.component.html',
  styleUrls: ['./group.component.scss']
})
export class GroupComponent implements OnInit {
  public group: OrgGroup;

  constructor(private route: ActivatedRoute,
              private router: Router,
              private groupService: GroupService) {
  }

  ngOnInit(): void {
    this.route.data.subscribe((value: { group: OrgGroup }) => {
      this.group = value.group;
    });
  }

  async save(): Promise<void> {
    await this.groupService.updateGroup(this.group);
    this.router.navigate(['/groups']);
  }
}
