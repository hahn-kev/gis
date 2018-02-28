import { Component, OnInit, ViewChild } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { MatSidenav } from '@angular/material';
import { Router } from '@angular/router';
import { CookieService } from 'ngx-cookie';
import { LoginService } from './services/auth/login.service';
import { ActivityIndicatorService } from './services/activity-indicator.service';
import { SettingsService } from './services/settings.service';
import { UserToken } from './login/user-token';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {

  currentUser: Observable<UserToken>;
  indicatorStatus: Observable<boolean>;
  version: Observable<string>;
  hasTitle = false;
  @ViewChild('sidenav')
  private sidenav: MatSidenav;

  constructor(private loginService: LoginService,
              private router: Router,
              private cookieService: CookieService,
              activityIndicatorService: ActivityIndicatorService,
    settings: SettingsService) {
    this.currentUser = loginService.currentUserToken();
    this.indicatorStatus = activityIndicatorService.observeIndicator();
    this.version = settings.getAsync<string>('version');
  }

  ngOnInit(): void {
    this.router.events.subscribe(() => this.sidenav.close());
  }

  logout(): void {
    this.loginService.setLoggedIn(null);
  this.cookieService.remove('.JwtAccessToken');
  this.loginService.promptLogin();
}

}
