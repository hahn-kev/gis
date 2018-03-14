import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Self } from './self';
import { LoginService } from '../../services/auth/login.service';
import { Observable } from 'rxjs/Observable';
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

}
