import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { OrgGroup } from '../groups/org-group';
import { Person } from 'app/people/person';

@Component({
  selector: 'app-leave-request',
  templateUrl: './leave-request.component.html',
  styleUrls: ['./leave-request.component.scss']
})
export class LeaveRequestComponent implements OnInit {
  public people: Person[];
  public groups: OrgGroup[];
  public personLeaving: Person;
  public leaveStartDate: Date;
  public leaveEndDate: Date;

  constructor(private route: ActivatedRoute,
    private router: Router) {
  }

  ngOnInit(): void {
    this.route.data.subscribe((value) => {
      this.groups = value.groups;
      this.people = value.people;
    });
  }

}
