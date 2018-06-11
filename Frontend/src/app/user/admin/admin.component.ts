import { Component, OnInit } from '@angular/core';
import { User } from '../user';
import { UserService } from '../user.service';
import { AppDataSource } from '../../classes/app-data-source';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent implements OnInit {
  dataSource: AppDataSource<User>;

  constructor(private userService: UserService) {
    this.dataSource = new AppDataSource<User>();
  }

  ngOnInit() {
    this.dataSource.observe(this.userService.getUsers());
  }
}
