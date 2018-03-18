import { Component, OnInit, ViewChild } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { MatSidenav } from '@angular/material';
import { Router } from '@angular/router';
import { CookieService } from 'ngx-cookie';
import { LoginService } from './services/auth/login.service';
import { ActivityIndicatorService } from './services/activity-indicator.service';
import { SettingsService } from './services/settings.service';
import { UserToken } from './login/user-token';
import { AttachmentService } from './components/attachments/attachment.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {

  currentUser: Observable<UserToken>;
  indicatorStatus: Observable<boolean>;
  version: string;
  hasAttachments: Observable<boolean>;
  hasTitle = false;
  @ViewChild('sidenav')
  private sidenav: MatSidenav;
  @ViewChild('rightDrawer')
  private rightDrawer: MatSidenav;

  constructor(private loginService: LoginService,
              private router: Router,
              private attachmentService: AttachmentService,
              private cookieService: CookieService,
              activityIndicatorService: ActivityIndicatorService,
              settings: SettingsService) {
    this.currentUser = loginService.currentUserToken();
    this.indicatorStatus = activityIndicatorService.observeIndicator();
    this.version = settings.get<string>('version');
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

}
