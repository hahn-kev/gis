import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LeaveRequest } from '../leave-request';
import { AppDataSource } from 'app/classes/app-data-source';

@Component({
  selector: 'app-leave-list',
  templateUrl: './leave-list.component.html',
  styleUrls: ['./leave-list.component.scss']
})
export class LeaveListComponent implements OnInit {
  public dataSource: AppDataSource<LeaveRequest>;

//todo let non hr people access this, and filter by logged in user
  constructor(private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<LeaveRequest>();
    this.dataSource.bindToRouteData(this.route, 'leave');
  }

}
