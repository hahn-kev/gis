import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { OrgGroup } from '../groups/org-group';
import { Person } from 'app/people/person';
import { LeaveRequestService } from './leave-request.service';
import { LeaveRequest } from './leave-request';
import { MatSnackBar } from '@angular/material';
import { LoginService } from '../../services/auth/login.service';
import { Subscription } from 'rxjs/Subscription';

@Component({
  selector: 'app-leave-request',
  templateUrl: './leave-request.component.html',
  styleUrls: ['./leave-request.component.scss']
})
export class LeaveRequestComponent implements OnInit, OnDestroy {
  public people: Person[];
  public groups: OrgGroup[];
  public leaveRequest = new LeaveRequest();

  private userTokenSubscription: Subscription;

  constructor(private route: ActivatedRoute,
    private router: Router,
    private leaveRequestService: LeaveRequestService,
    private snackBar: MatSnackBar,
    public loginService: LoginService) {


  }

  ngOnInit(): void {
    this.route.data.subscribe((value) => {
      this.groups = value.groups;
      this.people = value.people;
      this.userTokenSubscription = this.loginService.safeUserToken().subscribe(user => {
        if (!this.people) return;
        const person = this.people.find(eachPerson => eachPerson.email === user.email);
        if (person) this.leaveRequest.personId = person.id;
      });
    });
  }

  ngOnDestroy(): void {
    this.userTokenSubscription.unsubscribe();
  }

  async request(): Promise<void> {
    const notified = await this.leaveRequestService.requestLeave(this.leaveRequest);
    this.snackBar.open(`Leave request created, notified ${notified.firstName} ${notified.lastName}`);
  }
}
