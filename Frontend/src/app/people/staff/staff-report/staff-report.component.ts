import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../../classes/app-data-source';
import { Gender, NationalityName, PersonWithStaff } from '../../person';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog, MatSort } from '@angular/material';
import * as moment from 'moment';
import { MomentInput } from 'moment';
import { RenderTemplateDialogComponent } from '../../../components/render-template-dialog/render-template-dialog.component';

@Component({
  selector: 'app-staff-report',
  templateUrl: './staff-report.component.html',
  styleUrls: ['./staff-report.component.scss']
})
export class StaffReportComponent implements OnInit {
  public nationalityName = NationalityName;
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
    'staff.endorsementAgency',
    'legalStatus',
    'isThai',
    'speaksEnglish',
    'nationality'
  ];
  public selectedColumns: string[];
  showThai = true;
  showNonThai = true;
  showMen = true;
  showWomen = true;
  filterOlderThan = false;
  olderThanFilter = 0;
  filterYoungerThan = false;
  youngerThanFilter = 0;
  @ViewChild(MatSort) sort: MatSort;

  constructor(private route: ActivatedRoute, private router: Router, private dialog: MatDialog) {
    this.selectedColumns = this.route.snapshot.queryParams['columns'] || ['preferredName', 'lastName'];
  }

  async openFilterDialog() {
    await RenderTemplateDialogComponent.OpenWait(this.dialog, 'Filters', 'filter');
    this.dataSource.filterUpdated();
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<PersonWithStaff>();
    this.dataSource.customColumnAccessor('country', data => data.isThai ? 'Thailand' : data.passportCountry);
    this.dataSource.customColumnAccessor('age', data => moment(data.birthdate).unix());
    this.dataSource.customColumnAccessor('untilBirthday', data => moment(data.birthdate).unix());
    this.dataSource.sort = this.sort;
    this.dataSource.customFilter = (data: PersonWithStaff) => this.matchesFilters(data);
    this.dataSource.bindToRouteData(this.route, 'staff');
    this.dataSource.filterPredicate = (data: PersonWithStaff, filter: string) => {
      return (data.preferredName.toUpperCase().startsWith(filter)
        || data.lastName.toUpperCase().startsWith(filter)
        || data.firstName.toUpperCase().startsWith(filter));
    };
  }

  matchesFilters(person: PersonWithStaff) {
    if (!this.showThai && person.isThai) return false;
    if (!this.showNonThai && !person.isThai) return false;
    if (!this.showMen && person.gender == Gender.Male) return false;
    if (!this.showWomen && person.gender == Gender.Female) return false;
    let yearsOld = this.age(person.birthdate);
    if ((this.filterYoungerThan || this.filterOlderThan) && Number.isNaN(yearsOld)) return false;
    if (this.filterOlderThan && this.olderThanFilter >= yearsOld) return false;
    if (this.filterYoungerThan && this.youngerThanFilter <= yearsOld) return false;
    return true;
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
