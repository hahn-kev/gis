import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Self } from './self';
import { LoginService } from '../../services/auth/login.service';
import { Observable } from 'rxjs';
import { UserToken } from '../../login/user-token';

@Component({
  selector: 'app-self',
  templateUrl: './self.component.html',
  styleUrls: ['./self.component.scss']
})
export class SelfComponent implements OnInit {
  public self: Self;
  public userToken: Observable<UserToken>;

  constructor(private route: ActivatedRoute, private loginService: LoginService) {
    this.userToken = this.loginService.currentUserToken();
  }

  ngOnInit() {
    this.route.data.subscribe((value: { self: Self }) => this.self = value.self);
  }

  formatPassportAddress() {
    return `${this.self.person.passportAddress}\n`
      + `${this.self.person.passportCity}, ${this.self.person.passportState}\n`
      + `${this.self.person.passportZip}, ${this.self.person.passportCountry}`;
  }
  formatThaiAddress() {
    return `${this.self.person.thaiAddress}\n`
      + `${this.self.person.thaiSoi}, ${this.self.person.thaiMubaan}\n`
      + `${this.self.person.thaiTambon}, ${this.self.person.thaiAmphur}\n`
      + `${this.self.person.thaiProvince}, ${this.self.person.thaiZip}`;
  }
}
