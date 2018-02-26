import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LeaveRequestService } from './leave-request.service';
import { LeaveRequest } from './leave-request';
import { MatDialog, MatSnackBar } from '@angular/material';
import { LoginService } from '../../services/auth/login.service';
import { Subscription } from 'rxjs/Subscription';
import { PersonService } from '../person.service';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/combineLatest';
import { UserToken } from '../../login/user-token';
import 'rxjs/add/operator/defaultIfEmpty';
import 'rxjs/add/operator/concat';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';
import { PersonAndLeaveDetails } from './person-and-leave-details';

@Component({
  selector: 'app-leave-request',
  templateUrl: './leave-request.component.html',
  styleUrls: ['./leave-request.component.scss']
})
export class LeaveRequestComponent implements OnInit, OnDestroy {
  public people: PersonAndLeaveDetails[];
  public leaveRequest: LeaveRequest;
  public selectedPerson: PersonAndLeaveDetails;
  public isNew: boolean;

  private subscription: Subscription;

  constructor(private route: ActivatedRoute,
              private router: Router,
              private leaveRequestService: LeaveRequestService,
              private snackBar: MatSnackBar,
              public loginService: LoginService,
              private personService: PersonService,
              private dialog: MatDialog) {
  }

  ngOnInit(): void {
    //we're adding an empty list at the beginning of this observable
    //so that we get a result right away, then later update with value
    const peopleWithLeaveObservable = Observable.of([]).concat(this.leaveRequestService.listPeopleWithLeave(false));
    this.subscription = this.route.data.combineLatest(this.loginService.safeUserToken(), peopleWithLeaveObservable)
      .subscribe(([data, user, people]: [{ leaveRequest: LeaveRequest }, UserToken, PersonAndLeaveDetails[]]) => {
        this.people = people;
        this.leaveRequest = data.leaveRequest;
        if (!this.people) return;
        const person = this.people.find(
          eachPerson => eachPerson.person.id === (this.leaveRequest.personId || user.personId));
        if (person) {
          this.leaveRequest.personId = person.person.id;
          this.selectedPerson = person;
        } else if (this.people.length > 0) {
          //todo clean this up so we're not doing it twice
          this.showAllPeople();
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
    this.selectedPerson = this.people.find(value => value.person.id === personId);
  }

  async deleteRequest(): Promise<void> {
    const result = await this.dialog.open(ConfirmDialogComponent,
      {
        data: ConfirmDialogComponent.Options(`Delete Request for ${this.selectedPerson.person.preferredName}?`,
          'Delete',
          'Cancel')
      }).afterClosed().toPromise();
    if (!result) return;
    await this.leaveRequestService.deleteRequest(this.leaveRequest.id).toPromise();

    this.snackBar.open('Request deleted');
    this.router.navigate(['../..', 'list'], {relativeTo: this.route});
  }

  async showAllPeople() {
    this.people = await this.leaveRequestService.listPeopleWithLeave(true).toPromise();
    this.personSelectedChanged(this.leaveRequest.personId);
  }
}
