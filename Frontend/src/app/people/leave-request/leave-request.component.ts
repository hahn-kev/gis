import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { OrgGroup } from '../groups/org-group';
import { Person } from 'app/people/person';
import { LeaveRequestService } from './leave-request.service';
import { LeaveRequest } from './leave-request';
import { MatSnackBar } from '@angular/material';

@Component({
  selector: 'app-leave-request',
  templateUrl: './leave-request.component.html',
  styleUrls: ['./leave-request.component.scss']
})
export class LeaveRequestComponent implements OnInit {
  public people: Person[];
  public groups: OrgGroup[];
  public personLeavingId: string;
  public leaveStartDate: Date;
  public leaveEndDate: Date;

  constructor(private route: ActivatedRoute,
              private router: Router,
              private leaveRequestService: LeaveRequestService,
              private snackBar: MatSnackBar) {
  }

  ngOnInit(): void {
    this.route.data.subscribe((value) => {
      this.groups = value.groups;
      this.people = value.people;
    });
  }

  async request() {
    let notified = await this.leaveRequestService.requestLeave(new LeaveRequest(undefined, this.personLeavingId, this.leaveStartDate, this.leaveEndDate));
    this.snackBar.open(`Leave request created, notified ${notified.firstName} ${notified.lastName}`);
  }
}
