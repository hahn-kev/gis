import { Component } from '@angular/core';
import { LoginService } from 'app/services/auth/login.service';
import { Observable } from 'rxjs/Observable';
import { UserToken } from '../login/user-token'
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {
  public userToken: Observable<UserToken>;

  constructor(private loginService: LoginService, private http: HttpClient) {
    this.userToken = this.loginService.safeUserToken();
  }

  makeError() {
    this.http.get('/api/test/error').subscribe();
  }
}
