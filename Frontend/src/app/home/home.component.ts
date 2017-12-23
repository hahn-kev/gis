import { Component } from '@angular/core';
import { LoginService } from 'app/services/auth/login.service';
import { Observable } from 'rxjs/Observable';
import { UserToken } from '../login/user-token'

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  public userToken: Observable<UserToken>;

  constructor(private loginService: LoginService) {
    this.userToken = this.loginService.safeUserToken();
  }

}
