import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { APP_INITIALIZER, ErrorHandler, NgModule } from '@angular/core';
import { HTTP_INTERCEPTORS, HttpClient, HttpClientModule } from '@angular/common/http';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { UserComponent } from './user/user.component';
import { AppRoutingModule } from './app-routing.module';
import { UserService } from './user/user.service';
import { LoginComponent } from './login/login.component';
import { AdminComponent } from './user/admin/admin.component';
import { HomeComponent } from './home/home.component';
import {
  MatAutocompleteModule,
  MatBadgeModule,
  MatBottomSheetModule,
  MatButtonModule,
  MatButtonToggleModule,
  MatCardModule,
  MatCheckboxModule,
  MatChipsModule,
  MatDatepickerModule,
  MatDialogModule,
  MatExpansionModule,
  MatGridListModule,
  MatIconModule,
  MatInputModule,
  MatListModule,
  MatMenuModule,
  MatOptionModule,
  MatProgressBarModule,
  MatSelectModule,
  MatSidenavModule,
  MatSlideToggleModule,
  MatSnackBarModule,
  MatSortModule,
  MatTableModule,
  MatToolbarModule,
  MatTooltipModule,
  MatTreeModule
} from '@angular/material';
import { MatMomentDateModule } from '@angular/material-moment-adapter';
import { ActivityIndicatorService } from './services/activity-indicator.service';
import { ActivityIndicatorInterceptorService } from './services/activity-indicator-interceptor.service';
import { ObserversModule } from '@angular/cdk/observers';
import { ToolbarTemplateDirective } from './toolbar/toolbar-template.directive';
import { ToolbarContentDirective } from './toolbar/toolbar-content.directive';
import { AppTemplateService } from './toolbar/app-template.service';
import { MyErrorHandlerService } from './services/my-error-handler.service';
import { configureSettings, SettingsService } from './services/settings.service';
import { ConfirmDialogComponent } from './dialog/confirm-dialog/confirm-dialog.component';
import { MessageComponent } from './message/message.component';
import { CookieService } from 'ngx-cookie-service';
import { RequireRoleDirective } from './directives/require-role.directive';
import { AuthenciateInterceptorService } from './services/auth/authenciate-interceptor.service';
import { LoginService } from 'app/services/auth/login.service';
import { AuthenticateService } from './services/auth/authenticate.service';
import { RoleGuardService } from 'app/services/auth/role-guard.service';
import * as Raven from 'raven-js';
import { PersonComponent } from './people/person/person.component';
import { PeopleListComponent } from './people/list/people-list.component';
import { PersonService } from './people/person.service';
import { RoleComponent } from './people/person/role.component';
import { environment } from '../environments/environment';
import { RolesReportComponent } from './people/roles-report/roles-report.component';
import { OrgGroupListComponent } from './people/groups/org-group-list/org-group-list.component';
import { GroupService } from './people/groups/group.service';
import { GroupComponent } from './people/groups/group/group.component';
import { LeaveRequestComponent } from './people/leave-request/leave-request.component';
import { LeaveRequestService } from './people/leave-request/leave-request.service';
import { LeaveListComponent } from './people/leave-request/leave-list/leave-list.component';
import { TrainingListComponent } from './people/training-requirement/training-list/training-list.component';
import { TrainingEditComponent } from './people/training-requirement/training-edit/training-edit.component';
import { TrainingRequirementService } from './people/training-requirement/training-requirement.service';
import { TrainingReportComponent } from './people/training-requirement/training-report/training-report.component';
import { StaffTrainingComponent } from './people/person/staff-training/staff-training.component';
import { EmergencyContactComponent } from './people/person/emergency-contact/emergency-contact.component';
import { SelfComponent } from './people/self/self.component';
import { SelfService } from './people/self/self.service';
import { QuickAddComponent } from './people/person/quick-add/quick-add.component';
import { QuickAddDirective } from './people/person/quick-add/quick-add.directive';
import { AppTemplateContentDirective } from './directives/app-template-content.directive';
import { LeaveReportComponent } from './people/leave-request/leave-report/leave-report.component';
import { GooglePickerComponent } from './google-picker/google-picker.component';
import { GoogleApiModule, NG_GAPI_CONFIG } from 'ng-gapi';
import { DrivePickerService } from './google-picker/drive-picker.service';
import { AttachmentsComponent } from './components/attachments/attachments.component';
import { AttachmentService } from './components/attachments/attachment.service';
import { StaffReportComponent } from './people/staff/staff-report/staff-report.component';
import { JobListComponent } from './job/list/job-list.component';
import { JobComponent } from './job/job/job.component';
import { JobService } from './job/job.service';
import { MissionOrgListComponent } from './mission-org/list/mission-org-list.component';
import { MissionOrgComponent } from './mission-org/edit/mission-org.component';
import { MissionOrgService } from './mission-org/mission-org.service';
import { GradeListComponent } from './job/grade/list/grade-list.component';
import { GradeComponent } from './job/grade/edit/grade.component';
import { GradeService } from './job/grade/grade.service';
import { RenderTemplateDialogComponent } from './components/render-template-dialog/render-template-dialog.component';
import { MyDatePipe } from './services/my-date.pipe';
import { LocationStrategy, PathLocationStrategy } from '@angular/common';
import { ExportButtonComponent } from './components/export-button/export-button.component';
import { CsvService } from './services/csv.service';
import { SandboxComponent } from './components/sandbox/sandbox.component';
import { EvaluationComponent } from './people/person/evaluation/evaluation.component';
import { EvaluationService } from './people/person/evaluation/evaluation.service';
import { TitleCasePipe } from './services/title-case.pipe';
import { QuickAddButtonComponent } from './people/person/quick-add/quick-add-button.component';
import { AccordionListComponent } from './components/accordion-list/accordion-list.component';
import { AccordionListHeaderDirective } from './components/accordion-list/accordion-list-header.directive';
import { AccordionListContentDirective } from './components/accordion-list/accordion-list-content.directive';
import { AccordionListFormDirective } from './components/accordion-list/accordion-list-form.directive';
import { RenderTemplateBottomSheetComponent } from './components/render-template-bottom-sheet/render-template-bottom-sheet.component';
import { OrgTreeComponent } from './org-tree/org-tree.component';
import { DialogDirective } from './components/render-template-dialog/dialog.directive';
import { EvaluationReportComponent } from './people/evaluation-report/evaluation-report.component';
import { ProfilePictureComponent } from './people/person/profile-picture/profile-picture.component';
import { PickFileDirective } from './google-picker/pick-file.directive';
import { DonorComponent } from './people/person/donor/donor.component';
import { EndorsementListComponent } from './endorsement/list/endorsement-list.component';
import { EndorsementComponent } from './endorsement/edit/endorsement.component';
import { IsUserPolicyPipe } from './services/auth/is-user-policy.pipe';
import { PolicyGuard } from './services/auth/policy.guard';
import { DonationComponent } from './people/person/donor/donation.component';
import { CalendarComponent } from './calendar/calendar.component';
import { EducationComponent } from './people/person/education/education.component';
import { DateAddPipe } from './services/date-add.pipe';
import { AccordionListCustomActionDirective } from './components/accordion-list/accordion-list-custom-action.directive';
import { BottomSheetDirective } from './components/render-template-bottom-sheet/bottom-sheet.directive';
import { GroupTypeNamePipe } from './people/groups/group-type-name.pipe';
import { IsLastVisiblePipe } from './org-tree/is-last-visible.pipe';
import { FindFormPipe } from './components/accordion-list/find-form.pipe';
import {SchoolAidListComponent} from './people/schoolAidList/school-aid-list.component';

if (environment.production) {
  Raven.config('https://026d43df17b245588298bfa5ac8aa333@sentry.io/249854', {environment: 'production'}).install();
}

@NgModule({
  declarations: [
    AppComponent,
    UserComponent,
    LoginComponent,
    AdminComponent,
    HomeComponent,
    RequireRoleDirective,
    ToolbarTemplateDirective,
    ToolbarContentDirective,
    ConfirmDialogComponent,
    MessageComponent,
    PersonComponent,
    PeopleListComponent,
    RoleComponent,
    RolesReportComponent,
    OrgGroupListComponent,
    GroupComponent,
    LeaveRequestComponent,
    LeaveListComponent,
    TrainingListComponent,
    TrainingEditComponent,
    TrainingReportComponent,
    StaffTrainingComponent,
    EmergencyContactComponent,
    SelfComponent,
    QuickAddComponent,
    QuickAddDirective,
    AppTemplateContentDirective,
    LeaveReportComponent,
    GooglePickerComponent,
    AttachmentsComponent,
    StaffReportComponent,
    JobListComponent,
    JobComponent,
    MissionOrgListComponent,
    MissionOrgComponent,
    GradeListComponent,
    GradeComponent,
    RenderTemplateDialogComponent,
    MyDatePipe,
    ExportButtonComponent,
    SandboxComponent,
    EvaluationComponent,
    TitleCasePipe,
    QuickAddButtonComponent,
    AccordionListComponent,
    AccordionListHeaderDirective,
    AccordionListContentDirective,
    AccordionListFormDirective,
    RenderTemplateBottomSheetComponent,
    DialogDirective,
    OrgTreeComponent,
    EvaluationReportComponent,
    ProfilePictureComponent,
    PickFileDirective,
    DonorComponent,
    TitleCasePipe,
    EndorsementListComponent,
    EndorsementComponent,
    IsUserPolicyPipe,
    DonationComponent,
    CalendarComponent,
    EducationComponent,
    DateAddPipe,
    AccordionListCustomActionDirective,
    BottomSheetDirective
    ,
    GroupTypeNamePipe
    ,
    IsLastVisiblePipe
    ,
    FindFormPipe,
    SchoolAidListComponent
  ],
  entryComponents: [
    ConfirmDialogComponent,
    QuickAddComponent,
    RenderTemplateDialogComponent,
    RenderTemplateBottomSheetComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    BrowserAnimationsModule,
    MatButtonModule,
    MatInputModule,
    MatOptionModule,
    MatSelectModule,
    MatSidenavModule,
    MatToolbarModule,
    MatTooltipModule,
    MatCardModule,
    MatListModule,
    MatDialogModule,
    MatDatepickerModule,
    MatMomentDateModule,
    MatTableModule,
    MatSortModule,
    MatIconModule,
    MatCheckboxModule,
    MatSnackBarModule,
    MatProgressBarModule,
    MatTableModule,
    ObserversModule,
    MatButtonToggleModule,
    MatChipsModule,
    MatMenuModule,
    MatSlideToggleModule,
    MatExpansionModule,
    MatAutocompleteModule,
    MatGridListModule,
    MatBottomSheetModule,
    MatTreeModule,
    MatBadgeModule,
    FormsModule,
    AppRoutingModule,
    GoogleApiModule.forRoot({
      provide: NG_GAPI_CONFIG,
      deps: [SettingsService],
      useFactory: (settings: SettingsService) => ({client_id: settings.get<string>('googleClientId')})
    })
  ],
  providers: [
    UserService,
    LoginService,
    AuthenticateService,
    AuthenciateInterceptorService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthenciateInterceptorService,
      multi: true
    },
    ActivityIndicatorService,
    ActivityIndicatorInterceptorService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ActivityIndicatorInterceptorService,
      multi: true
    },
    RoleGuardService,
    AppTemplateService,
    {
      provide: ErrorHandler,
      useClass: MyErrorHandlerService
    },
    SettingsService,
    PersonService,
    GroupService,
    LeaveRequestService,
    TrainingRequirementService,
    SelfService,
    DrivePickerService,
    {
      provide: APP_INITIALIZER,
      useFactory: configureSettings,
      deps: [HttpClient, SettingsService],
      multi: true
    },
    AttachmentService,
    JobService,
    MissionOrgService,
    GradeService,
    MyDatePipe,
    CsvService,
    {
      provide: LocationStrategy,
      useClass: PathLocationStrategy
    },
    EvaluationService,
    PolicyGuard,
    CookieService
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
