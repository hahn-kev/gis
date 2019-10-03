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
import { ActivityIndicatorService } from './services/activity-indicator.service';
import { ActivityIndicatorInterceptorService } from './services/activity-indicator-interceptor.service';
import { AppTemplateService } from './toolbar/app-template.service';
import { MyErrorHandlerService } from './services/my-error-handler.service';
import { configureSettings, SettingsService } from './services/settings.service';
import { ConfirmDialogComponent } from './dialog/confirm-dialog/confirm-dialog.component';
import { MessageComponent } from './message/message.component';
import { RequireRoleDirective } from './directives/require-role.directive';
import { AuthenciateInterceptorService } from './services/auth/authenciate-interceptor.service';
import { LoginService } from 'app/services/auth/login.service';
import { AuthenticateService } from './services/auth/authenticate.service';
import { RoleGuardService } from 'app/services/auth/role-guard.service';
import { PersonService } from './people/person.service';
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
import { SelfComponent } from './people/self/self.component';
import { SelfService } from './people/self/self.service';
import { LeaveReportComponent } from './people/leave-request/leave-report/leave-report.component';
import { GooglePickerComponent } from './google-picker/google-picker.component';
import { GoogleApiModule, NG_GAPI_CONFIG } from 'ng-gapi';
import { DrivePickerService } from './google-picker/drive-picker.service';
import { AttachmentsComponent } from './attachments/attachments.component';
import { AttachmentService } from './attachments/attachment.service';
import { JobService } from './job/job.service';
import { MissionOrgListComponent } from './mission-org/list/mission-org-list.component';
import { MissionOrgComponent } from './mission-org/edit/mission-org.component';
import { MissionOrgService } from './mission-org/mission-org.service';
import { GradeService } from './job/grade/grade.service';
import { MyDatePipe } from './services/my-date.pipe';
import { LocationStrategy, PathLocationStrategy } from '@angular/common';
import { CsvService } from './services/csv.service';
import { SandboxComponent } from './components/sandbox/sandbox.component';
import { EvaluationService } from './people/person/evaluation/evaluation.service';
import { RenderTemplateBottomSheetComponent } from './components/render-template-bottom-sheet/render-template-bottom-sheet.component';
import { OrgTreeComponent } from './org-tree/org-tree.component';
import { PickFileDirective } from './google-picker/pick-file.directive';
import { EndorsementListComponent } from './endorsement/list/endorsement-list.component';
import { EndorsementComponent } from './endorsement/edit/endorsement.component';
import { PolicyGuard } from './services/auth/policy.guard';
import { CalendarComponent } from './calendar/calendar.component';
import { BottomSheetDirective } from './components/render-template-bottom-sheet/bottom-sheet.directive';
import { GroupTypeNamePipe } from './people/groups/group-type-name.pipe';
import { IsLastVisiblePipe } from './org-tree/is-last-visible.pipe';
import { AppComponentsModule } from './components/app-components.module';
import { HolidayListComponent } from './holiday/list/holiday-list.component';
import { HolidayComponent } from './holiday/edit/holiday.component';

@NgModule({
  declarations: [
    AppComponent,
    UserComponent,
    LoginComponent,
    AdminComponent,
    HomeComponent,
    RequireRoleDirective,
    ConfirmDialogComponent,
    MessageComponent,
    OrgGroupListComponent,
    GroupComponent,
    LeaveRequestComponent,
    LeaveListComponent,
    TrainingListComponent,
    TrainingEditComponent,
    TrainingReportComponent,
    SelfComponent,
    LeaveReportComponent,
    GooglePickerComponent,
    AttachmentsComponent,
    MissionOrgListComponent,
    MissionOrgComponent,
    MyDatePipe,
    SandboxComponent,
    RenderTemplateBottomSheetComponent,
    OrgTreeComponent,
    PickFileDirective,

    EndorsementListComponent,
    EndorsementComponent,
    CalendarComponent,
    BottomSheetDirective,
    GroupTypeNamePipe,
    IsLastVisiblePipe,
    HolidayListComponent,
    HolidayComponent
  ],
  entryComponents: [
    ConfirmDialogComponent,
    RenderTemplateBottomSheetComponent
  ],
  imports: [
    AppComponentsModule,
    AppRoutingModule,
    //3rd party modules
    BrowserModule,
    HttpClientModule,
    BrowserAnimationsModule,
    FormsModule,
    GoogleApiModule.forRoot({
      provide: NG_GAPI_CONFIG,
      deps: [SettingsService],
      useFactory: (settings: SettingsService) => ({
        apiKey: settings.get<string>('googleAPIKey'),
        client_id: settings.get<string>('googleClientId'),
        scope: 'https://www.googleapis.com/auth/drive'
      })
    })
  ],
  exports: [],
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
    PolicyGuard
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
