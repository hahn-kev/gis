import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LeaveRequestWithNames } from '../leave-request';
import { AppDataSource } from 'app/classes/app-data-source';
import { LoginService } from '../../../services/auth/login.service';
import { MatSort } from '@angular/material';
import { Year } from '../../training-requirement/year';
import { UrlBindingService } from '../../../services/url-binding.service';

@Component({
  selector: 'app-leave-list',
  templateUrl: './leave-list.component.html',
  styleUrls: ['./leave-list.component.scss'],
  providers: [UrlBindingService]
})
export class LeaveListComponent implements OnInit {
  public dataSource = new AppDataSource<LeaveRequestWithNames>();
  public yearNameFromDate = Year.schoolYearNameFromDate;
  public filteredByUser: string | null;
  public showingMine: boolean;
  public title: string;
  public hrColumns = [
    'requesterName',
    'type',
    'approved',
    'approvedByName',
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
    'approvedByName',
    'createdDate',
    'schoolYear'
  ];
  @ViewChild(MatSort) matSort: MatSort;

  constructor(private route: ActivatedRoute, public loginService: LoginService,
              private urlBinding: UrlBindingService<{ search: string, showApproved: boolean }>) {
    this.route.params.subscribe(value => {
      this.filteredByUser = value['personId'];
    });
    this.route.url.subscribe(url => {
      this.showingMine = url.some(value => value.path === 'mine');
    });

    this.urlBinding.addParam('search', '')
      .subscribe(value => this.dataSource.filter = value.toUpperCase());
    this.urlBinding.addParam('showApproved', false);
    this.urlBinding.onParamsUpdated = values => this.dataSource.filterUpdated();
    this.dataSource.customFilter = value => {
      if (!this.urlBinding.values.showApproved && value.approved) return false;
      return true;
    };
    this.dataSource.customColumnAccessor('schoolYear', data => Year.schoolYear(data.startDate));
    this.dataSource.bindToRouteData(this.route, 'leave');
    this.dataSource.filterPredicate = (data, filter) => data.requesterName.toUpperCase().includes(filter);
    if (!this.urlBinding.loadFromParams()) this.dataSource.filterUpdated();
  }

  ngOnInit(): void {
    this.dataSource.sort = this.matSort;
  }

}
