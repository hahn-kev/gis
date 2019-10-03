import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Observable } from 'rxjs';
import { MatBottomSheet } from '@angular/material/bottom-sheet';
import { MatDialog } from '@angular/material/dialog';
import { MatSidenav } from '@angular/material/sidenav';
import { Router, RouterEvent } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { LoginService } from './services/auth/login.service';
import { ActivityIndicatorService } from './services/activity-indicator.service';
import { SettingsService } from './services/settings.service';
import { UserToken } from './login/user-token';
import { AttachmentService } from './attachments/attachment.service';
import { Meta, Title } from '@angular/platform-browser';
import { environment } from '../environments/environment';
import { JobStatus, NonSchoolAidJobStatus } from './job/job';
import { delay, map } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  public isDev = !environment.production;
  nonSchoolAidJobStatus = NonSchoolAidJobStatus;
  schoolAidJobStatus = JobStatus.SchoolAid;
  currentUser: Observable<UserToken>;
  indicatorStatus: Observable<boolean>;
  version: string;
  hasAttachments: Observable<boolean>;
  hasTitle = false;
  @ViewChild('sidenav', {static: true})
  private sidenav: MatSidenav;
  @ViewChild('rightDrawer', {static: true})
  private rightDrawer: MatSidenav;
  @ViewChild('titleElement', {static: true})
  private titleElement: ElementRef;

  constructor(private loginService: LoginService,
              private router: Router,
              private attachmentService: AttachmentService,
              private cookieService: CookieService,
              activityIndicatorService: ActivityIndicatorService,
              settings: SettingsService,
              private titleService: Title,
              private bottomSheet: MatBottomSheet,
              private dialog: MatDialog,
              meta: Meta) {
    this.currentUser = loginService.currentUserToken();
    this.indicatorStatus = activityIndicatorService.observeIndicator().pipe(delay(1));
    this.version = settings.get<string>('version');
    meta.addTag({name: 'theme-color', content: '#283593'});
  }

  ngOnInit(): void {
    this.router.events.subscribe((e: RouterEvent) => {
      this.sidenav.close();
      if (this.rightDrawer) this.rightDrawer.close();
      this.bottomSheet.dismiss();
      //todo figure out how to let dialogs change search parameters without closing dialogs
      // this.dialog.closeAll();
    });
    this.hasAttachments = this.attachmentService.extractId().pipe(map(value => value.hasAttachments));
  }

  logout(): void {
    this.loginService.setLoggedIn(null);
    this.cookieService.delete('.JwtAccessToken');
    this.loginService.promptLogin();
  }

  updateTitle() {
    this.titleService.setTitle(this.titleElement.nativeElement.innerText);
  }

  isLinkActive(url: string): boolean {
    return this.router.url.includes(url);
  }

}
