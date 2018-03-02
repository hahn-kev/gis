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

@Component({
  selector: 'app-leave-request',
  templateUrl: './leave-request.component.html',
  styleUrls: ['./leave-request.component.scss']
})
export class LeaveRequestComponent implements OnInit, OnDestroy {
  public people: PersonAndLeaveDetails[];
  public leaveRequest: LeaveRequestWithNames;
  public daysUsed = 0;
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
    this.subscription = this.route.data.combineLatest(this.loginService.safeUserToken())
      .subscribe(([data, user]: [{ leaveRequest: LeaveRequestWithNames, people: PersonAndLeaveDetails[] }, UserToken, PersonAndLeaveDetails[]]) => {
        this.people = data.people;
        this.leaveRequest = data.leaveRequest;
        this.daysUsed = this.leaveRequestService.weekDays(this.leaveRequest);
        const person = this.people.find(p => p.person.id === (this.leaveRequest.personId || user.personId));
        if (person) {
          this.leaveRequest.personId = person.person.id;
          this.selectedPerson = person;
        }
      });
    this.isNew = this.route.snapshot.params['id'] === 'new';
  }

  updateDaysUsed() {
    this.daysUsed = this.leaveRequestService.weekDays(this.leaveRequest);
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  async submit(): Promise<void> {
    if (this.isNew) {
      if (this.leaveRequestService.isOverUsingLeave(this.leaveRequest, this.selectedPerson.leaveUseages)) {

        const result = await ConfirmDialogComponent.OpenWait(this.dialog,
          `This leave request is using more leave than you have`,
          'Continue',
          'Cancel');
        if (!result) return;
      }
      const notified = await this.leaveRequestService.requestLeave(this.leaveRequest);
      if (!notified) {
        this.snackBar.open(`Leave request created, supervisor not found, no notification was sent`, null, {duration: 2000});
      } else {
        this.snackBar.open(`Leave request created, notified ${notified.firstName} ${notified.lastName}`, null, {duration: 2000});
      }
    } else {
      await this.leaveRequestService.updateLeave(this.leaveRequest).toPromise();
      this.snackBar.open('Leave updated, notification was not sent of changes', null, {duration: 2000});
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
}
