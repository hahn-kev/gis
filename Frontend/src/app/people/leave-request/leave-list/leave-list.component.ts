import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LeaveRequest } from '../leave-request';
import { AppDataSource } from 'app/classes/app-data-source';
import { LoginService } from '../../../services/auth/login.service';
import { LeaveTypeName } from '../../self/self';
import { MatSort } from '@angular/material';
import { Year } from '../../training-requirement/year';

@Component({
  selector: 'app-leave-list',
  templateUrl: './leave-list.component.html',
  styleUrls: ['./leave-list.component.scss']
})
export class LeaveListComponent implements OnInit {
  public dataSource: AppDataSource<LeaveRequest>;
  public yearNameFromDate = Year.schoolYearNameFromDate;
  public typeName = LeaveTypeName;
  public filteredByUser: string | null;
  public showingMine: boolean;
  public hrColumns = [
    'requester',
    'type',
    'approved',
    'approvedBy',
    'startDate',
    'endDate',
    'days',
    'createdDate',
    'schoolYear'
  ];
  public personColumns = [
    'startDate',
    'endDate',
    'days',
    'type',
    'approved',
    'approvedBy',
    'createdDate',
    'schoolYear'
  ];
  @ViewChild(MatSort) matSort: MatSort;

  constructor(private route: ActivatedRoute, public loginService: LoginService) {
    this.route.params.subscribe(value => {
      this.filteredByUser = value['personId'];
      this.showingMine = this.filteredByUser === 'mine';
    });
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<LeaveRequest>();
    this.dataSource.sort = this.matSort;
    this.dataSource.bindToRouteData(this.route, 'leave');
  }

}
