import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LoginService } from './login.service';
import { User } from '../../user/user';

@Injectable()
export class AuthenticateService {

  constructor(private http: HttpClient, private loginService: LoginService) {
  }

  async login(username: string, currentPassword: string, newPassword?: string): Promise<User> {

    let body = {
      Username: username,
      Password: currentPassword
    };
    if (newPassword) body['NewPassword'] = newPassword;
    let json = await this.http.post<any>(`/api/authenticate/${newPassword ? 'reset' : 'signin'}`, body).toPromise();
    let user = Object.assign(new User(), json.user);
    if (!user.resetPassword) {
      //we won't get an access token back if the users password needs to be reset
      this.loginService.setLoggedIn(json.access_token);
    }
    return user;
  }
}
