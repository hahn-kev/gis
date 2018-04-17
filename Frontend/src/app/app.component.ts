import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { MatSidenav } from '@angular/material';
import { Router } from '@angular/router';
import { CookieService } from 'ngx-cookie';
import { LoginService } from './services/auth/login.service';
import { ActivityIndicatorService } from './services/activity-indicator.service';
import { SettingsService } from './services/settings.service';
import { UserToken } from './login/user-token';
import { AttachmentService } from './components/attachments/attachment.service';
import { Meta, Title } from '@angular/platform-browser';
import { environment } from '../environments/environment';
import { NonSchoolAidJobTypes } from './job/job';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  public isDev = !environment.production;
  nonSchoolAidJobTypes = NonSchoolAidJobTypes;
  currentUser: Observable<UserToken>;
  indicatorStatus: Observable<boolean>;
  version: string;
  hasAttachments: Observable<boolean>;
  hasTitle = false;
  @ViewChild('sidenav')
  private sidenav: MatSidenav;
  @ViewChild('rightDrawer')
  private rightDrawer: MatSidenav;
  @ViewChild('titleElement')
  private titleElement: ElementRef;

  constructor(private loginService: LoginService,
              private router: Router,
              private attachmentService: AttachmentService,
              private cookieService: CookieService,
              activityIndicatorService: ActivityIndicatorService,
              settings: SettingsService,
              private titleService: Title,
              meta: Meta) {
    this.currentUser = loginService.currentUserToken();
    this.indicatorStatus = activityIndicatorService.observeIndicator();
    this.version = settings.get<string>('version');
    meta.addTag({name:'theme-color', content: '#283593'});
  }

  ngOnInit(): void {
    this.router.events.subscribe(() => {
      this.sidenav.close();
      if (this.rightDrawer) this.rightDrawer.close();
    });
    this.hasAttachments = this.attachmentService.extractId().map(value => value.hasAttachments);
  }

  logout(): void {
    this.loginService.setLoggedIn(null);
    this.cookieService.remove('.JwtAccessToken');
    this.loginService.promptLogin();
  }

  updateTitle() {
    this.titleService.setTitle(this.titleElement.nativeElement.innerText);
  }

  isLinkActive(url: string): boolean {
    return this.router.url.includes(url);
  }

}
