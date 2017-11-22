import { Component, OnInit } from '@angular/core';
import { User } from '../user';
import { UserService } from '../user.service';
import { Router } from '@angular/router';
import { Observable } from 'rxjs/Observable';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent implements OnInit {
  users: Observable<User[]>;
  constructor(private userService: UserService, private router: Router) { }

  async ngOnInit() {
    this.users = await this.userService.getUsers();
  }
}
