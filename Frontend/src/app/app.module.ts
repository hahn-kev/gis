import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { APP_INITIALIZER, ErrorHandler, NgModule } from '@angular/core';
import { HTTP_INTERCEPTORS, HttpClient, HttpClientModule } from '@angular/common/http';
import { LocalStorageModule } from 'angular-2-local-storage';
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
  MatTableModule,
  MatToolbarModule,
  MatTooltipModule
} from '@angular/material';
import { MatMomentDateModule } from '@angular/material-moment-adapter';
import { ActivityIndicatorService } from './services/activity-indicator.service';
import { ActivityIndicatorInterceptorService } from './services/activity-indicator-interceptor.service';
import { CdkTableModule } from '@angular/cdk/table';
import { ToolbarTemplateDirective } from './toolbar/toolbar-template.directive';
import { ToolbarContentDirective } from './toolbar/toolbar-content.directive';
import { AppTemplateService } from './toolbar/app-template.service';
import { MyErrorHandlerService } from './services/my-error-handler.service';
import { configureSettings, SettingsService } from './services/settings.service';
import { ClipboardModule } from 'ngx-clipboard/dist';
import { ConfirmDialogComponent } from './dialog/confirm-dialog/confirm-dialog.component';
import { MessageComponent } from './message/message.component';
import { CookieModule } from 'ngx-cookie';
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
    GooglePickerComponent
  ],
  entryComponents: [
    ConfirmDialogComponent,
    QuickAddComponent
  ],
  imports: [
    LocalStorageModule.withConfig({
      prefix: 'app',
      storageType: 'localStorage'
    }),
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
    MatIconModule,
    MatCheckboxModule,
    MatSnackBarModule,
    MatProgressBarModule,
    MatTableModule,
    CdkTableModule,
    MatButtonToggleModule,
    MatChipsModule,
    MatMenuModule,
    MatSlideToggleModule,
    MatExpansionModule,
    MatAutocompleteModule,
    MatGridListModule,
    FormsModule,
    AppRoutingModule,
    ClipboardModule,
    CookieModule.forRoot(),
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
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
