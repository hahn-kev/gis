import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { ErrorHandler, NgModule } from '@angular/core';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { LocalStorageModule } from 'angular-2-local-storage';

import { AppComponent } from './app.component';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import 'hammerjs';
import { UserComponent } from './user/user.component';
import { AppRoutingModule } from './app-routing.module';
import { UserService } from './user/user.service';
import { LoginComponent } from './login/login.component';
import { AdminComponent } from './user/admin/admin.component';
import { HomeComponent } from './home/home.component';
import {
  MatButtonModule,
  MatButtonToggleModule,
  MatCardModule,
  MatCheckboxModule,
  MatChipsModule,
  MatDatepickerModule,
  MatDialogModule,
  MatIconModule,
  MatInputModule,
  MatListModule,
  MatMenuModule,
  MatNativeDateModule,
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
import { FlipCardComponent } from './home/flip-card/flip-card.component';
import { ActivityIndicatorService } from './services/activity-indicator.service';
import { ActivityIndicatorInterceptorService } from './services/activity-indicator-interceptor.service';
import { MarkdownModule } from 'angular2-markdown';
import { CdkTableModule } from '@angular/cdk/table';
import { ToolbarTemplateDirective } from './toolbar/toolbar-template.directive';
import { ToolbarContentDirective } from './toolbar/toolbar-content.directive';
import { ToolbarService } from './toolbar/toolbar.service';
import { YourRightsComponent } from './home/static/your-rights.component';
import { LifeLessonsComponent } from './home/static/life-lessons.component';
import { MyErrorHandlerService } from './services/my-error-handler.service';
import { SettingsService } from './services/settings.service';
import { DiscourseLinkDirective } from './directives/discourse-link.directive';
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

Raven.config('https://026d43df17b245588298bfa5ac8aa333@sentry.io/249854').install();

@NgModule({
  declarations: [
    AppComponent,
    UserComponent,
    LoginComponent,
    AdminComponent,
    HomeComponent,
    FlipCardComponent,
    RequireRoleDirective,
    ToolbarTemplateDirective,
    ToolbarContentDirective,
    YourRightsComponent,
    LifeLessonsComponent,
    DiscourseLinkDirective,
    ConfirmDialogComponent,
    MessageComponent,
    PersonComponent,
    PeopleListComponent
  ],
  entryComponents: [
    ConfirmDialogComponent
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
    MatNativeDateModule,
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
    FormsModule,
    AppRoutingModule,
    MarkdownModule.forRoot(),
    ClipboardModule,
    CookieModule.forRoot()
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
    ToolbarService,
    {
      provide: ErrorHandler,
      useClass: MyErrorHandlerService
    },
    SettingsService,
    PersonService
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
