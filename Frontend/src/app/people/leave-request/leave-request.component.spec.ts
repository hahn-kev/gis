import { async, ComponentFixture, fakeAsync, TestBed } from '@angular/core/testing';

import { Location } from '@angular/common';
import { LeaveRequestComponent } from './leave-request.component';
import {
  MatCardModule,
  MatChipsModule,
  MatDatepickerModule,
  MatDialog,
  MatIconModule,
  MatInputModule,
  MatSelectModule,
  MatSlideToggleModule,
  MatSnackBar,
  MatTooltipModule
} from '@angular/material';
import { FormsModule } from '@angular/forms';
import { TitleCasePipe } from '../../services/title-case.pipe';
import { ActivatedRoute, Router } from '@angular/router';
import { LeaveRequestService } from './leave-request.service';
import { LoginService } from '../../services/auth/login.service';
import { PersonService } from '../person.service';
import { Directive, Input } from '@angular/core';
import { MatMomentDateModule } from '@angular/material-moment-adapter';
import { LeaveRequestWithNames } from './leave-request';
import { UserToken } from '../../login/user-token';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { HolidayService } from '../../holiday/holiday.service';
import SpyObj = jasmine.SpyObj;

@Directive({
  selector: '[appTemplateContent]'
})
// tslint:disable-next-line:component-class-suffix
class MockAppTemplateContentDirective {
  @Input('appTemplateContent') appTemplateContent: string;
}

describe('LeaveRequestComponent', () => {
  let component: LeaveRequestComponent;
  let fixture: ComponentFixture<LeaveRequestComponent>;
  let dialogSpy: SpyObj<MatDialog>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        MatSlideToggleModule,
        MatDatepickerModule,
        MatInputModule,
        MatCardModule,
        MatSelectModule,
        MatChipsModule,
        MatTooltipModule,
        MatMomentDateModule,
        MatIconModule,
        FormsModule,
        NoopAnimationsModule
      ],
      declarations: [LeaveRequestComponent, MockAppTemplateContentDirective, TitleCasePipe],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: {
            data: [{leaveRequest: new LeaveRequestWithNames(), people: []}],
            queryParams: [{noNotification: false}]
          }
        },
        {provide: MatDialog, useValue: jasmine.createSpyObj(['open'])},
        {provide: Router, useValue: {}},
        {provide: LeaveRequestService, useValue: new LeaveRequestService(null)},
        {provide: MatSnackBar, useValue: {}},
        {
          provide: LoginService, useValue: {
            safeUserToken: () => [new UserToken({})]
          }
        },
        {provide: HolidayService, useValue: {currentHolidays: () => of([])}},
        {provide: PersonService, useValue: {}},
        {provide: Location, useValue: {}},
      ]
    })
      .compileComponents();
  }));

  beforeEach(fakeAsync(async () => {
    fixture = TestBed.createComponent(LeaveRequestComponent);
    component = fixture.componentInstance;
    dialogSpy = TestBed.get(MatDialog);
    dialogSpy.open.and.callFake(() => {
      return {afterClosed: () => of(true)};
    });
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  }));

  function confirmDialogOpened(messageContains: string) {
    expect(dialogSpy.open).toHaveBeenCalledTimes(1);
    let callInfo = dialogSpy.open.calls.first();
    expect(callInfo.args[1].data.title).toContain(messageContains);
    dialogSpy.open.calls.reset();
  }

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('confirm dialogs', () => {

    it('should confirm send email dialogs', () => {
      component.leaveRequest.approved = true;
      component.promptSendNotification();
      expect(dialogSpy.open).not.toHaveBeenCalled();

      component.leaveRequest.approved = false;
      component.promptSendNotification();
      confirmDialogOpened('email?');
    });


  });
});
