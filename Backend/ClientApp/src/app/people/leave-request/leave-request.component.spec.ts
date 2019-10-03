import { async, ComponentFixture, fakeAsync, TestBed } from '@angular/core/testing';

import { Location } from '@angular/common';
import { LeaveRequestComponent } from './leave-request.component';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { TitleCasePipe } from '../../services/title-case.pipe';
import { ActivatedRoute, Router } from '@angular/router';
import { LeaveRequestService } from './leave-request.service';
import { LoginService } from '../../services/auth/login.service';
import { PersonService } from '../person.service';
import { MatMomentDateModule } from '@angular/material-moment-adapter';
import { LeaveRequestWithNames } from './leave-request';
import { UserToken } from '../../login/user-token';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { HolidayService } from '../../holiday/holiday.service';
import { MockAppTemplateContentDirective } from '../../directives/app-template-content.directive.spec';
import SpyObj = jasmine.SpyObj;

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
