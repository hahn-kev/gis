import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../../classes/app-data-source';
import { Gender, PersonWithStaffSummaries } from '../../person';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog, MatSort } from '@angular/material';
import * as moment from 'moment';
import { MomentInput } from 'moment';
import { UrlBindingService } from '../../../services/url-binding.service';

@Component({
  selector: 'app-staff-report',
  templateUrl: './staff-report.component.html',
  styleUrls: ['./staff-report.component.scss'],
  providers: [UrlBindingService]
})
export class StaffReportComponent implements OnInit {
  public dataSource: AppDataSource<PersonWithStaffSummaries>;
  public allOrgGroups: { name: string, superId: string }[] = [];
  public allMissionOrgs: string[] = [];
  age = StaffReportComponent.age;
  public avalibleColumns = [
    'preferredName',
    'firstName',
    'lastName',
    'thaiFirstName',
    'thaiLastName',
    'personalEmail',
    'staffEmail',
    'phoneNumber',
    'phoneExtension',
    'serviceLength',
    'isActive',
    'startDate',
    'department /Division',
    'sendingOrg',

    'birthdate',
    'age',
    'untilBirthday',
    'gender',
    'country',
    'legalStatus',
    'isThai',
    'speaksEnglish'
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
                age: number,
                ageType: string,
                serviceLength: number,
                serviceLengthType: string,
                group: string[],
                sendingOrg: string[]
              }>) {
    this.dataSource = new AppDataSource<PersonWithStaffSummaries>();
    this.dataSource.customColumnAccessor('country', data => data.isThai ? 'Thailand' : data.passportCountry);
    this.dataSource.customColumnAccessor('age', data => moment(data.birthdate).unix());
    this.dataSource.customColumnAccessor('untilBirthday', data => this.timeToBirthday(data.birthdate).unix());
    this.dataSource.customColumnAccessor('serviceLength', data => this.serviceLength(data).asDays());
    this.dataSource.bindToRouteData(this.route, 'staff');
    //filter list to distinct
    this.allOrgGroups = this.dataSource.filteredData
      .map(value => {
        return {name: value.staff.orgGroupName, superId: value.staff.orgGroupSupervisor};
      })
      .filter((value, index, array) => array.map(_ => _.name).indexOf(value.name) == index && value.name != null)
      .sort((a, b) => a.name.localeCompare(b.name));
    this.allMissionOrgs = this.dataSource.filteredData
      .map(value => value.staff.missionOrgName)
      .filter((value, index, array) => array.indexOf(value) == index && value != null)
      .sort();
    this.dataSource.filterPredicate = (data: PersonWithStaffSummaries, filter: string) => {
      return data.preferredName.toUpperCase().startsWith(filter)
        || data.lastName.toUpperCase().startsWith(filter)
        || data.firstName.toUpperCase().startsWith(filter)
        || (data.thaiFirstName || '').startsWith(filter)
        || (data.thaiLastName || '').startsWith(filter)
        || (data.email || '').toUpperCase().includes(filter)
        || (data.staff.email || '').toUpperCase().includes(filter);
    };
    this.dataSource.customFilter = person => {
      if (!this.urlBinding.values.showInactive && !person.isActive) return false;
      if (!this.urlBinding.values.showThai && person.isThai) return false;
      if (!this.urlBinding.values.showNonThai && !person.isThai) return false;
      if (!this.urlBinding.values.showMen && person.gender === Gender.Male) return false;
      if (!this.urlBinding.values.showWomen && person.gender === Gender.Female) return false;
      const yearsOld = StaffReportComponent.age(person.birthdate);
      if (!this.testNumber(this.urlBinding.values.ageType, this.urlBinding.values.age, yearsOld)) return false;
      if (!this.testNumber(this.urlBinding.values.serviceLengthType,
        this.urlBinding.values.serviceLength,
        this.serviceLength(person).asYears())) return false;
      if (!this.matchesOrgGroupFilter(person)) return false;
      if (this.urlBinding.values.sendingOrg.length > 0 &&
        !this.urlBinding.values.sendingOrg.includes(person.staff.missionOrgName)) return false;
      return true;
    };

    this.urlBinding.addParam('search', '').subscribe(value => this.dataSource.filter = value.toUpperCase());
    this.urlBinding.addParam('showInactive', false);
    this.urlBinding.addParam('selectedColumns', ['preferredName', 'lastName', 'personalEmail', 'phoneNumber', 'serviceLength']);
    this.urlBinding.addParam('showThai', true);
    this.urlBinding.addParam('showNonThai', true);
    this.urlBinding.addParam('showMen', true);
    this.urlBinding.addParam('showWomen', true);

    this.urlBinding.addParam('group', []);
    this.urlBinding.addParam('sendingOrg', []);
    this.urlBinding.addParam('age', 0);
    this.urlBinding.addParam('ageType', null);
    this.urlBinding.addParam('serviceLength', 0);
    this.urlBinding.addParam('serviceLengthType', null);
    this.urlBinding.onParamsUpdated = values => this.dataSource.filterUpdated();
    if (!this.urlBinding.loadFromParams()) this.dataSource.filterUpdated();
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

  serviceLength(person: PersonWithStaffSummaries) {
    return moment.duration(person.daysOfService / 365.25, 'years')
      .add(person.staff.yearsOfServiceAdjustment, 'years');
  }

  daysAsYears(person: PersonWithStaffSummaries) {
    if (person.daysOfService < 1 && person.staff.yearsOfServiceAdjustment === 0) return 'None';
    return this.serviceLength(person).humanize();
  }

  testNumber(type: string, constraint: number, test: number) {
    if (!type) return true;
    switch (type) {
      case '<':
        return test < constraint;
      case '>':
        return test > constraint;
      case '<>':
        return test != constraint;
      case '=':
        return test == constraint;
    }
  }

  matchesOrgGroupFilter(person: PersonWithStaffSummaries) {
    let groups = this.urlBinding.values.group;
    if (groups.length == 0) return true;
    if (groups.includes(person.staff.orgGroupName)) return true;
    let group = this.allOrgGroups.find(value => value.superId == person.id);
    if (!group) return false;
    return groups.includes(group.name);
  }
}
