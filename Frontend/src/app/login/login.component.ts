import { Component, Inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { User } from '../user/user';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LoginService } from '../services/auth/login.service';
import { AuthenticateService } from '../services/auth/authenticate.service';
import { DOCUMENT } from '@angular/common';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  username: string;
  password: string;
  newPassword: string;
  errorMessage: string;
  passwordReset = false;

  get redirectUrl(): string {
    return this.route.snapshot.queryParams['redirect'] || this.loginService.redirectTo;
  }

  constructor(private authenticateService: AuthenticateService,
              private loginService: LoginService,
              private router: Router,
              private route: ActivatedRoute,
              private snackBar: MatSnackBar,
              @Inject(DOCUMENT) private document: Document) {
  }

  ngOnInit(): void {
  }

  async login(): Promise<void> {
    this.errorMessage = '';
    let user: User;
    try {
      user = await this.authenticateService.login(this.username,
        this.password,
        this.passwordReset ? this.newPassword : null);
    } catch (errorResponse) {
      this.errorMessage = errorResponse.error.message;
      return;
    }
    if (user.resetPassword) {
      this.passwordReset = true;
      this.snackBar.open('Password reset required', 'Dissmiss', {duration: 2000});
      return;
    } else if (this.route.snapshot.queryParamMap.has('redirect')) {
      this.document.location.href = this.route.snapshot.queryParamMap.get('redirect');
    } else {
      let success = await this.router.navigate([this.loginService.redirectTo]);
      if (!success) this.router.navigate(['home']);
    }
  }
}
