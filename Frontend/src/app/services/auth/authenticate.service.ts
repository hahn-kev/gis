import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LoginService } from './login.service';
import { User } from '../../user/user';
import { CookieService } from 'ngx-cookie-service';

@Injectable()
export class AuthenticateService {

  constructor(private http: HttpClient, private loginService: LoginService, private cookieService: CookieService) {
  }

  async login(username: string, currentPassword: string, newPassword?: string): Promise<User> {

    let body = {
      Username: username,
      Password: currentPassword
    };
    if (newPassword) body['NewPassword'] = newPassword;
    let json: { user: User, access_token: string } =
      await this.http.post<any>(`/api/authenticate/${newPassword ? 'reset' : 'signin'}`, body).toPromise();
    return this.postLogin(json);
  }

  async impersonate(email: string) {
    let user = this.postLogin(await this.http.get<any>('/api/authenticate/impersonate/' + encodeURI(email))
      .toPromise());
    this.cookieService.delete('.Sub');
    return user;
  }

  private postLogin(json: { user: User, access_token: string }): User {
    let user = Object.assign(new User(), json.user);
    if (!user.resetPassword) {
      //we won't get an access token back if the users password needs to be reset
      this.loginService.setLoggedIn(json.access_token);
    }
    return user;
  }
}
