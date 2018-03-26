import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LeaveRequestService } from './leave-request.service';
import { LeaveRequestWithNames } from './leave-request';
import { MatDialog, MatSnackBar } from '@angular/material';
import { LoginService } from '../../services/auth/login.service';
import { Subscription } from 'rxjs/Subscription';
import { PersonService } from '../person.service';
import 'rxjs/add/operator/combineLatest';
import { UserToken } from '../../login/user-token';
import 'rxjs/add/operator/defaultIfEmpty';
import 'rxjs/add/operator/concat';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';
import { PersonAndLeaveDetails } from './person-and-leave-details';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/share';
import { LeaveType } from '../self/self';
import { Gender } from '../person';
import * as moment from 'moment';

@Component({
  selector: 'app-leave-request',
  templateUrl: './leave-request.component.html',
  styleUrls: ['./leave-request.component.scss']
})
export class LeaveRequestComponent implements OnInit, OnDestroy {
  public typesOfLeave = Object.keys(LeaveType);
  public people: PersonAndLeaveDetails[];
  public leaveRequest: LeaveRequestWithNames;
  public daysUsed = 0;
  public selectedPerson: PersonAndLeaveDetails;
  public isNew: boolean;
  public isHr: Observable<boolean>;
  private myPersonId: string | null;

  private subscription: Subscription;

  constructor(private route: ActivatedRoute,
              private router: Router,
              private leaveRequestService: LeaveRequestService,
              private snackBar: MatSnackBar,
              public loginService: LoginService,
              private personService: PersonService,
              private dialog: MatDialog) {
    this.isHr = this.loginService.isHrOrAdmin();
  }

  ngOnInit(): void {
    //we're adding an empty list at the beginning of this observable
    //so that we get a result right away, then later update with value
    this.subscription = this.route.data.combineLatest(this.loginService.safeUserToken())
      .subscribe(([data, user]: [
        { leaveRequest: LeaveRequestWithNames, people: PersonAndLeaveDetails[] },
        UserToken,
        PersonAndLeaveDetails[]
        ]) => {
        this.people = data.people;
        this.leaveRequest = data.leaveRequest;
        this.updateDaysUsed();
        this.myPersonId = user.personId;
        const person = this.people.find(p => p.person.id === (this.leaveRequest.personId || user.personId));
        if (person) {
          this.leaveRequest.personId = person.person.id;
          this.selectedPerson = person;
        }
      });
    this.isNew = this.route.snapshot.params['id'] === 'new';
  }

  updateDaysUsed() {
    if (!this.leaveRequest.overrideDays) {
      this.leaveRequest.days = this.leaveRequestService.weekDays(this.leaveRequest);
    }
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  async submit(): Promise<void> {
    if (this.isNew) {
      let overUsingLeave = this.leaveRequestService.isOverUsingLeave(this.leaveRequest, this.selectedPerson.leaveUseages);
      let doctorsNote = this.leaveRequest.type == LeaveType.Sick && (this.leaveRequest.days > 2);
      if (overUsingLeave || doctorsNote) {

        const result = await ConfirmDialogComponent.OpenWait(this.dialog,
          (overUsingLeave ? `This leave request is using more leave than you have\n` : '')
          + (doctorsNote ? `You must email a doctors note to HR` : ''),
          'Request Leave',
          'Cancel');
        if (!result) return;
      }
      const notified = await this.leaveRequestService.requestLeave(this.leaveRequest);
      if (!notified) {
        this.snackBar.open(`Leave request created, supervisor not found, no notification was sent`,
          null,
          {duration: 2000});
      } else {
        this.snackBar.open(`Leave request created, notified ${notified.firstName} ${notified.lastName}`,
          null,
          {duration: 2000});
      }
      this.router.navigate([
        'leave-request',
        'list',
        this.myPersonId === this.leaveRequest.personId ? 'mine' : this.leaveRequest.personId
      ])
    } else {
      await this.leaveRequestService.updateLeave(this.leaveRequest).toPromise();
      this.snackBar.open('Leave updated, notification was not sent of changes', null, {duration: 2000});
    }
  }

  personSelectedChanged(personId: string): void {
    this.selectedPerson = this.people.find(value => value.person.id === personId);
  }

  overrideDaysChanged(overrideDays: boolean) {
    if (!overrideDays) {
      this.updateDaysUsed();
    }
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
    this.router.navigate([
      'leave-request',
      'list',
      this.myPersonId === this.leaveRequest.personId ? 'mine' : this.leaveRequest.personId
    ]);
  }

  showLeaveType(leaveType: LeaveType) {
    switch (leaveType) {
      case LeaveType.Vacation:
        return this.selectedPerson.person.isThai;
      case LeaveType.Paternity:
        return this.selectedPerson.person.gender == Gender.Male;
      case LeaveType.Maternity:
        return this.selectedPerson.person.gender == Gender.Female;
      default:
        return true;
    }
  }
}
