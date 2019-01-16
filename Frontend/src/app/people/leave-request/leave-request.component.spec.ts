import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { Location } from '@angular/common';
import { LeaveRequestComponent } from './leave-request.component';
import {
  MatCardModule,
  MatChipsModule,
  MatDatepickerModule,
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
        {provide: Router, useValue: {}},
        {provide: LeaveRequestService, useValue: new LeaveRequestService(null)},
        {provide: MatSnackBar, useValue: {}},
        {
          provide: LoginService, useValue: {
            safeUserToken: () => [new UserToken({})]
          }
        },
        {provide: PersonService, useValue: {}},
        {provide: Location, useValue: {}},
      ]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LeaveRequestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
