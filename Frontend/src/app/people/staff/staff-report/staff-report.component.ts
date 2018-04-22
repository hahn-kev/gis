import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../../classes/app-data-source';
import { Gender, NationalityName, PersonWithStaff, PersonWithStaffSummaries } from '../../person';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { MatDialog, MatSort } from '@angular/material';
import * as moment from 'moment';
import { MomentInput } from 'moment';
import { RenderTemplateDialogComponent } from '../../../components/render-template-dialog/render-template-dialog.component';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { combineLatest } from 'rxjs/observable/combineLatest';
import { Subject } from 'rxjs/Subject';
import { debounceTime } from 'rxjs/operators';
import { ReplaySubject } from 'rxjs/ReplaySubject';

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
  filter = {
    text: new BehaviorSubject(''),
    showInactive: new BehaviorSubject(false),
    selectedColumns: new BehaviorSubject(['preferredName', 'lastName']),
    showThai: new BehaviorSubject(true),
    showNonThai: new BehaviorSubject(true),
    showMen: new BehaviorSubject(true),
    showWomen: new BehaviorSubject(true),
    filterOlderThan: new BehaviorSubject(false),
    olderThanFilter: new BehaviorSubject(0),
    filterYoungerThan: new BehaviorSubject(false),
    youngerThanFilter: new BehaviorSubject(0),
    matches: function (person: PersonWithStaffSummaries): boolean {
      if (!this.showInactive.value && !person.isActive) return false;
      if (!this.showThai.value && person.isThai) return false;
      if (!this.showNonThai.value && !person.isThai) return false;
      if (!this.showMen.value && person.gender == Gender.Male) return false;
      if (!this.showWomen.value && person.gender == Gender.Female) return false;
      let yearsOld = StaffReportComponent.age(person.birthdate);
      if ((this.filterYoungerThan.value || this.filterOlderThan.value) && Number.isNaN(yearsOld)) return false;
      if (this.filterOlderThan.value && this.olderThanFilter.value >= yearsOld) return false;
      if (this.filterYoungerThan.value && this.youngerThanFilter.value <= yearsOld) return false;
      return true;
    }
  };

  @ViewChild(MatSort) sort: MatSort;

  constructor(private route: ActivatedRoute, private router: Router, private dialog: MatDialog) {
  }

  async openFilterDialog() {
    await RenderTemplateDialogComponent.OpenWait(this.dialog, 'Filters', 'filter');
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<PersonWithStaffSummaries>();
    this.dataSource.customColumnAccessor('country', data => data.isThai ? 'Thailand' : data.passportCountry);
    this.dataSource.customColumnAccessor('age', data => moment(data.birthdate).unix());
    this.dataSource.customColumnAccessor('untilBirthday', data => this.timeToBirthday(data.birthdate).unix());
    this.dataSource.sort = this.sort;
    this.dataSource.customFilter = (data: PersonWithStaffSummaries) => this.filter.matches(data);
    this.dataSource.bindToRouteData(this.route, 'staff');
    this.dataSource.filterPredicate = (data: PersonWithStaffSummaries, filter: string) => {
      return (data.preferredName.toUpperCase().startsWith(filter)
        || data.lastName.toUpperCase().startsWith(filter)
        || data.firstName.toUpperCase().startsWith(filter));
    };
    let ignoreChanges = true;
    combineLatest(Object.keys(this.filter)
      .filter(key => key !== 'matches')
      .map(key => this.filter[key])
    )
    // .pipe(debounceTime(1))
      .subscribe((values) => {
        if (ignoreChanges) return;
        let params: Params = {};
        let keys = Object.keys(this.filter).filter(key => key !== 'matches');
        for (let i = 0; i < keys.length; i++) {
          params[keys[i]] = values[i];
        }
        this.router.navigate(['.'],
          {
            relativeTo: this.route,
            queryParams: params
          });
      });
    this.route.queryParams.subscribe(value => {
      let keys = Object.keys(value);
      ignoreChanges = true;
      for (let key of keys) {
        let subject: Subject<any> = this.filter[key];
        let keyValue = value[key];
        if (key != 'text' && (keyValue == 'false' || keyValue == 'true')) keyValue = keyValue != 'false';
        subject.next(keyValue);
      }
      ignoreChanges = false;
      this.dataSource.filter = this.filter.text.value.toUpperCase();
      this.dataSource.filterUpdated();
    });
  }

  age = StaffReportComponent.age;

  static age(date: MomentInput) {
    return moment().diff(moment(date), 'years');
  }

  timeToBirthday(date: MomentInput) {
    let mDate = moment(date).year(moment().year());
    if (mDate.isBefore(moment())) return mDate.add(1, 'year');
    return mDate;
  }

  daysAsYears(days: number){
    if (days < 1) return 'None';
    return moment.duration(days, 'days').humanize();
  }

  camel2title(camelCase: string) {
    return camelCase
      .replace(/([A-Z])/g, (match) => ` ${match}`)
      .replace(/^./, (match) => match.toUpperCase());
  }
}
