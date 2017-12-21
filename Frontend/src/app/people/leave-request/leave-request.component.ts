import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LeaveRequestService } from './leave-request.service';
import { LeaveRequest } from './leave-request';
import { MatSnackBar } from '@angular/material';
import { LoginService } from '../../services/auth/login.service';
import { Subscription } from 'rxjs/Subscription';
import { PersonService } from '../person.service';
import { PersonWithDaysOfLeave } from '../person';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/combineLatest';
import { UserToken } from '../../login/user-token';
import 'rxjs/add/operator/defaultIfEmpty';
import 'rxjs/add/operator/concat';

@Component({
  selector: 'app-leave-request',
  templateUrl: './leave-request.component.html',
  styleUrls: ['./leave-request.component.scss']
})
export class LeaveRequestComponent implements OnInit, OnDestroy {
  public people: PersonWithDaysOfLeave[];
  public leaveRequest: LeaveRequest;
  public selectedPerson: PersonWithDaysOfLeave;
  public isNew: boolean;

  private subscription: Subscription;

  constructor(private route: ActivatedRoute,
              private router: Router,
              private leaveRequestService: LeaveRequestService,
              private snackBar: MatSnackBar,
              public loginService: LoginService,
              private personService: PersonService) {
  }

  ngOnInit(): void {
    //we're adding an empty list at the beginning of this observable
    //so that we get a result right away, then later update with value
    const peopleWithLeaveObservable = Observable.of([]).concat(this.personService.getPeopleWithDaysOfLeave());
    this.subscription = this.route.data.combineLatest(this.loginService.safeUserToken(), peopleWithLeaveObservable)
      .subscribe(([data, user, people]: [{ leaveRequest: LeaveRequest }, UserToken, PersonWithDaysOfLeave[]]) => {
        this.people = people;
        this.leaveRequest = data.leaveRequest;
        if (!this.people) return;
        const person = this.people.find(eachPerson => eachPerson.id === (this.leaveRequest.personId || user.personId));
        if (person) {
          this.leaveRequest.personId = person.id;
          this.selectedPerson = person;
        }
      });
    this.isNew = this.route.snapshot.params['id'] === 'new';
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  async submit(): Promise<void> {
    if (this.isNew) {
      const notified = await this.leaveRequestService.requestLeave(this.leaveRequest);
      this.snackBar.open(`Leave request created, notified ${notified.firstName} ${notified.lastName}`);
    } else {
      await this.leaveRequestService.updateLeave(this.leaveRequest).toPromise();
      this.snackBar.open('Leave updated, notification was not sent of changes');
    }
  }

  personSelectedChanged(personId: string): void {
    this.selectedPerson = this.people.find(value => value.id === personId);
  }
}
