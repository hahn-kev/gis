import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LeaveRequestService } from './leave-request.service';
import { LeaveRequest } from './leave-request';
import { MatSnackBar } from '@angular/material';
import { LoginService } from '../../services/auth/login.service';
import { Subscription } from 'rxjs/Subscription';
import { PersonService } from '../person.service';
import { PersonWithDaysOfLeave } from '../person';

@Component({
  selector: 'app-leave-request',
  templateUrl: './leave-request.component.html',
  styleUrls: ['./leave-request.component.scss']
})
export class LeaveRequestComponent implements OnInit, OnDestroy {
  public people: PersonWithDaysOfLeave[];
  public leaveRequest = new LeaveRequest();
  public daysOfLeaveUsed: number;

  private userTokenSubscription: Subscription;

  constructor(private route: ActivatedRoute,
              private router: Router,
              private leaveRequestService: LeaveRequestService,
              private snackBar: MatSnackBar,
              public loginService: LoginService,
              private personService: PersonService) {
  }

  async ngOnInit(): Promise<void> {
    this.people = await this.personService.getPeopleWithDaysOfLeave().toPromise();
    this.userTokenSubscription = this.loginService.safeUserToken().subscribe(user => {
      if (!this.people) return;
      const person = this.people.find(eachPerson => eachPerson.id === user.personId);
      if (person) {
        this.leaveRequest.personId = person.id;
        this.daysOfLeaveUsed = person.daysOfLeaveUsed;
      }
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
