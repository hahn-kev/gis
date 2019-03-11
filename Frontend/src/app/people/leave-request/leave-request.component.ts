import { Component, OnDestroy, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { LeaveRequestService } from './leave-request.service';
import { LeaveRequestWithNames } from './leave-request';
import { MatDialog, MatSnackBar, MatSnackBarRef, SimpleSnackBar } from '@angular/material';
import { LoginService } from '../../services/auth/login.service';
import { combineLatest, Subscription } from 'rxjs';
import { PersonService } from '../person.service';
import { UserToken } from '../../login/user-token';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';
import { PersonAndLeaveDetails } from './person-and-leave-details';
import { LeaveType, LeaveUsage } from '../self/self';
import { Gender } from '../person';
import { BaseEditComponent } from '../../components/base-edit-component';
import { Holiday } from './holiday';

@Component({
  selector: 'app-leave-request',
  templateUrl: './leave-request.component.html',
  styleUrls: ['./leave-request.component.scss']
})
export class LeaveRequestComponent extends BaseEditComponent implements OnInit, OnDestroy {
  public typesOfLeave = Object.keys(LeaveType);
  public people: PersonAndLeaveDetails[];
  public leaveRequest: LeaveRequestWithNames;
  public daysUsed = 0;
  public selectedPerson: PersonAndLeaveDetails | null;
  public isNew: boolean;
  public isHr = false;
  public sendNotification = true;
  private myPersonId: string | null;
  private noNotificationSnackbarRef: MatSnackBarRef<SimpleSnackBar> = null;
  holidays: Holiday[];

  private subscription: Subscription;

  get isReadonly() {
    return this.leaveRequest.approved !== null && !this.isHr;
  }

  constructor(private route: ActivatedRoute,
              private router: Router,
              private leaveRequestService: LeaveRequestService,
              private snackBar: MatSnackBar,
              public loginService: LoginService,
              private personService: PersonService,
              private location: Location,
              dialog: MatDialog) {
    super(dialog);
  }

  ngOnInit(): void {
    this.subscription = combineLatest(this.route.data, this.loginService.safeUserToken(), this.route.queryParams)
      .subscribe(([data, user, queryParams]: [
        { leaveRequest: LeaveRequestWithNames, people: PersonAndLeaveDetails[] },
        UserToken,
        { noNotification: boolean }
        ]) => {
        this.people = data.people;
        this.leaveRequest = data.leaveRequest;
        this.isNew = !this.leaveRequest.id;
        this.isHr = user.isHrOrAdmin();
        this.updateDaysUsed();
        this.sendNotification = (!this.isHr || (!queryParams.noNotification && this.isHr));
        if (!this.sendNotification && this.isNew) {
          setTimeout(() => this.warnNoLeaveNotification());
        }
        this.myPersonId = user.personId;
        const person = this.people.find(p => p.person.id === (this.leaveRequest.personId || user.personId));
        if (person) {
          this.leaveRequest.personId = person.person.id;
          this.selectedPerson = person;
        }
      });
      this.leaveRequestService.listHolidays().subscribe(holidays => this.holidays = holidays);
  }

  warnNoLeaveNotification(sendNotification = false) {
    if (sendNotification) {
      if (this.noNotificationSnackbarRef)
        this.noNotificationSnackbarRef.dismiss();
      return;
    }
    this.noNotificationSnackbarRef = this.snackBar.open(
      `Approval Emails won't be sent for this leave request`,
      'Dismiss');
  }

  updateDaysUsed() {
    if ((this.leaveRequest.startDate && !this.leaveRequest.endDate) || this.leaveRequestService.isStartAfterEnd(this.leaveRequest)) {
      this.leaveRequest.endDate = this.leaveRequest.startDate;
    }
    if (this.leaveRequest && !this.leaveRequest.overrideDays && this.leaveRequest.startDate && this.leaveRequest.endDate) {
        this.leaveRequest.days = this.leaveRequestService.weekDays(this.leaveRequest, this.holidays);
      }
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  async submit(isDraft = false): Promise<void> {
    if (this.isReadonly) return;
    if (isDraft) this.formSubmitted = true;
    if (this.isNew && !await this.promptNewRequest()) {
      return;
    }
    if (!isDraft && this.sendNotification) {
      await this.submitLeaveRequest();
    } else {
      await this.saveRequest(isDraft);
    }
  }

  private async promptNewRequest() {
    let overUsingLeave = this.leaveRequestService.isOverUsingLeave(
      this.leaveRequest,
      this.selectedPerson.leaveUsages);
    let doctorsNote = this.leaveRequest.type == LeaveType.Sick && (this.leaveRequest.days > 2);
    if (overUsingLeave || doctorsNote) {

      return await ConfirmDialogComponent.OpenWait(
        this.dialog,
        (overUsingLeave ? `This leave request is using more leave than you have\n` : '')
        + (doctorsNote ? `You must email a doctors note to HR` : ''),
        'Request Leave',
        'Cancel');
    }
    return true;
  }

  private async submitLeaveRequest() {
    const notified = await this.leaveRequestService.requestLeave(this.leaveRequest);
    let message: string;
    if (!notified) {
      message = `Leave request ${this.isNew ? 'created' : 'updated'}, supervisor not found, no notification was sent`;
    } else {
      message = `Leave request ${this.isNew ?
        'created' :
        'updated'}, notified ${notified.firstName} ${notified.lastName}`;
    }
    this.snackBar.open(message, 'Dismiss');
    this.router.navigate([
      'leave-request',
      'list',
      this.myPersonId === this.leaveRequest.personId ? 'mine' : this.leaveRequest.personId
    ]);
  }

  private async saveRequest(isDraft: boolean) {
    await this.leaveRequestService.updateLeave(this.leaveRequest).toPromise();
    let message = 'Leave updated, notification was not sent of changes';
    if (isDraft) message = 'Leave Request draft saved';
    this.snackBar.open(message, null, {duration: 2000});
    this.location.back();
  }

  promptSendNotification() {
    if (this.leaveRequest.approved) return Promise.resolve(false);
    return ConfirmDialogComponent.OpenWait(
      this.dialog,
      'Send another leave request approval email?',
      'Send Email & Save',
      'Save');
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
    const result = await ConfirmDialogComponent.OpenWait(
      this.dialog,
      `Delete Request for ${this.selectedPerson.person.preferredName}?`,
      'Delete',
      'Cancel');
    if (!result) return;
    await this.leaveRequestService.deleteRequest(this.leaveRequest.id).toPromise();

    this.snackBar.open('Request deleted', null, {duration: 2000});
    this.location.back();
  }

  showLeaveUsage(leaveUsage: LeaveUsage) {
    if (!this.showLeaveType(leaveUsage.leaveType)) return false;
    return leaveUsage.totalAllowed != 0 || leaveUsage.used != 0;
  }

  showLeaveType(leaveType: LeaveType) {
    if (!this.selectedPerson) return true;
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
