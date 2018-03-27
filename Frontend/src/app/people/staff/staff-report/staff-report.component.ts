import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../../classes/app-data-source';
import { PersonWithStaff } from '../../person';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSort } from '@angular/material';
import * as moment from 'moment';
import { MomentInput } from 'moment';

@Component({
  selector: 'app-staff-report',
  templateUrl: './staff-report.component.html',
  styleUrls: ['./staff-report.component.scss']
})
export class StaffReportComponent implements OnInit {
  public dataSource: AppDataSource<PersonWithStaff>;
  public avalibleColumns = [
    'preferredName',
    'firstName',
    'lastName',
    'email',
    'phoneNumber',
    'birthdate',
    'age',
    'untilBirthday',
    'gender',
    'country',
    'staff.endorsementAgency'
  ];
  public selectedColumns: string[];
  @ViewChild(MatSort) sort: MatSort;

  constructor(private route: ActivatedRoute, private router: Router) {
    this.selectedColumns = this.route.snapshot.queryParams['columns'] || ['preferredName', 'lastName'];
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<PersonWithStaff>();
    this.dataSource.customColumnAccessor('country', data => data.isThai ? 'Thailand' : data.passportCountry);
    this.dataSource.customColumnAccessor('age', data => moment(data.birthdate).unix());
    this.dataSource.customColumnAccessor('untilBirthday', data => moment(data.birthdate).unix());
    this.dataSource.sort = this.sort;
    this.dataSource.bindToRouteData(this.route, 'staff');
    this.dataSource.filterPredicate = (data: PersonWithStaff, filter: string) => {
      return data.preferredName.toUpperCase().startsWith(filter)
        || data.lastName.toUpperCase().startsWith(filter)
        || data.firstName.toUpperCase().startsWith(filter);
    };
  }

  updateSelectedColumns(columns: string[]) {
    this.router.navigate(['.'], {
      relativeTo: this.route,
      queryParams: {'columns': columns}
    });
  }

  applyFilter(filterValue: string) {
    this.dataSource.filter = filterValue.trim().toUpperCase();
  }

  age(date: MomentInput) {
    return moment().diff(moment(date), 'years');
  }

  timeToBirthday(date: MomentInput) {
    return moment(date).year(moment().year()).fromNow();
  }
}
