import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../../classes/app-data-source';
import { Gender, NationalityName, PersonWithStaffSummaries } from '../../person';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog, MatSort } from '@angular/material';
import * as moment from 'moment';
import { MomentInput } from 'moment';
import { RenderTemplateDialogComponent } from '../../../components/render-template-dialog/render-template-dialog.component';
import { UrlBindingService } from '../../../services/url-binding.service';

@Component({
  selector: 'app-staff-report',
  templateUrl: './staff-report.component.html',
  styleUrls: ['./staff-report.component.scss'],
  providers: [UrlBindingService]
})
export class StaffReportComponent implements OnInit {
  public nationalityName = NationalityName;
  public dataSource: AppDataSource<PersonWithStaffSummaries>;
  age = StaffReportComponent.age;
  public avalibleColumns = [
    'preferredName',
    'firstName',
    'lastName',
    'thaiFirstName',
    'thaiLastName',
    'email',
    'staffEmail',
    'phoneNumber',
    'phoneExtension',
    'serviceLength',
    'isActive',

    'birthdate',
    'age',
    'untilBirthday',
    'gender',
    'country',
    'endorsementAgency',
    'legalStatus',
    'isThai',
    'speaksEnglish',
    'nationality'
  ];


  @ViewChild(MatSort) sort: MatSort;

  constructor(private route: ActivatedRoute,
              private router: Router,
              private dialog: MatDialog,
              public urlBinding: UrlBindingService<{
                search: string,
                showInactive: boolean,
                selectedColumns: string[],
                showThai: boolean,
                showNonThai: boolean,
                showMen: boolean,
                showWomen: boolean,
                filterOlderThan: boolean,
                olderThanFilter: number,
                filterYoungerThan: boolean,
                youngerThanFilter: number
              }>) {
    this.dataSource = new AppDataSource<PersonWithStaffSummaries>();
    this.dataSource.customColumnAccessor('country', data => data.isThai ? 'Thailand' : data.passportCountry);
    this.dataSource.customColumnAccessor('age', data => moment(data.birthdate).unix());
    this.dataSource.customColumnAccessor('untilBirthday', data => this.timeToBirthday(data.birthdate).unix());
    this.dataSource.bindToRouteData(this.route, 'staff');
    this.dataSource.filterPredicate = (data: PersonWithStaffSummaries, filter: string) => {
      return (data.preferredName.toUpperCase().startsWith(filter)
        || data.lastName.toUpperCase().startsWith(filter)
        || data.firstName.toUpperCase().startsWith(filter));
    };
    this.dataSource.customFilter = person => {
      if (!this.urlBinding.values.showInactive && !person.isActive) return false;
      if (!this.urlBinding.values.showThai && person.isThai) return false;
      if (!this.urlBinding.values.showNonThai && !person.isThai) return false;
      if (!this.urlBinding.values.showMen && person.gender === Gender.Male) return false;
      if (!this.urlBinding.values.showWomen && person.gender === Gender.Female) return false;
      const yearsOld = StaffReportComponent.age(person.birthdate);
      if ((this.urlBinding.values.filterYoungerThan || this.urlBinding.values.filterOlderThan) && Number.isNaN(yearsOld)) return false;
      if (this.urlBinding.values.filterOlderThan && this.urlBinding.values.olderThanFilter >= yearsOld) return false;
      if (this.urlBinding.values.filterYoungerThan && this.urlBinding.values.youngerThanFilter <= yearsOld) return false;
      return true;
    };

    this.urlBinding.addParam('search', '').subscribe(value => this.dataSource.filter = value.toUpperCase());
    this.urlBinding.addParam('showInactive', false);
    this.urlBinding.addParam('selectedColumns', ['preferredName', 'lastName']);
    this.urlBinding.addParam('showThai', true);
    this.urlBinding.addParam('showNonThai', true);
    this.urlBinding.addParam('showMen', true);
    this.urlBinding.addParam('showWomen', true);
    this.urlBinding.addParam('filterOlderThan', false);
    this.urlBinding.addParam('olderThanFilter', 0);
    this.urlBinding.addParam('filterYoungerThan', false);
    this.urlBinding.addParam('youngerThanFilter', 0);
    this.urlBinding.onParamsUpdated = values => this.dataSource.filterUpdated();
    if (!this.urlBinding.loadFromParams()) this.dataSource.filterUpdated();
  }

  async openFilterDialog() {
    await RenderTemplateDialogComponent.OpenWait(this.dialog, 'Filters', 'filter');
  }

  ngOnInit(): void {
    this.dataSource.sort = this.sort;
  }


  static age(date: MomentInput) {
    return moment().diff(moment(date), 'years');
  }

  timeToBirthday(date: MomentInput) {
    let mDate = moment(date).year(moment().year());
    if (mDate.isBefore(moment())) return mDate.add(1, 'year');
    return mDate;
  }

  daysAsYears(days: number) {
    if (days < 1) return 'None';
    return moment.duration(days, 'days').humanize();
  }
}
