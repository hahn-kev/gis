import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../../classes/app-data-source';
import { PersonWithStaff } from '../../person';
import { ActivatedRoute } from '@angular/router';
import { MatSort } from '@angular/material';
import * as moment from 'moment';

@Component({
  selector: 'app-staff-report',
  templateUrl: './staff-report.component.html',
  styleUrls: ['./staff-report.component.scss']
})
export class StaffReportComponent implements OnInit {
  public dataSource: AppDataSource<PersonWithStaff>;
  public avalibleColumns: Array<string> = [
    'firstName',
    'lastName',
    'email',
    'phoneNumber',
    'birthdate',
    'age',
    'gender',
    'passportCountry',
    'staff.endorsementAgency'
  ];
  public selectedColumns: Array<string> = ['firstName', 'lastName'];
  @ViewChild(MatSort) sort: MatSort;

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<PersonWithStaff>();
    this.dataSource.sort = this.sort;
    this.dataSource.bindToRouteData(this.route, 'staff');
    this.dataSource.filterPredicate = ((data, filter) =>
      data.firstName.toUpperCase().startsWith(filter) || data.lastName.toUpperCase().startsWith(filter));
  }

  applyFilter(filterValue: string) {
    this.dataSource.filter = filterValue.trim().toUpperCase();
  }

  age(date: string) {
    return moment().diff(moment(date), 'years');
  }
}
