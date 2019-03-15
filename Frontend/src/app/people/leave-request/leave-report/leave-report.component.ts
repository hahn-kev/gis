import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../../classes/app-data-source';
import { PersonAndLeaveDetails } from '../person-and-leave-details';
import { ActivatedRoute } from '@angular/router';
import { UrlBindingService } from '../../../services/url-binding.service';
import { PersonLeaveModel } from './person-leave-model';
import { MatSort } from '@angular/material';
import { Year } from '../../training-requirement/year';

@Component({
  selector: 'app-leave-report',
  templateUrl: './leave-report.component.html',
  styleUrls: ['./leave-report.component.scss'],
  providers: [UrlBindingService]
})
export class LeaveReportComponent implements OnInit {
  public years = Year.years();
  public dataSource = new AppDataSource<PersonLeaveModel>();
  public allOrgGroups: string[] = [];
  public allMissionOrgs: string[] = [];
  public isSupervisor = false;
  @ViewChild(MatSort) sort: MatSort;

  public columns = [
    'staff',
    'person.staff.orgGroupName',
    'person.staff.missionOrgName',
    'sick.used',
    'vacation.used',
    'personal.used',
    'parental.used',
    'emergency.used',
    'schoolRelated.used',
    'missionRelated.used',
    'other.used',
  ];
  exportColumns = [
    ...this.columns,
    'sick.totalAllowed',
    'vacation.totalAllowed',
    'personal.totalAllowed',
    'parental.totalAllowed',
    'emergency.totalAllowed',
    'schoolRelated.totalAllowed',
    'missionRelated.totalAllowed',
    'other.totalAllowed',
  ];

  constructor(private route: ActivatedRoute,
              public urlBinding: UrlBindingService<{
                search: string,
                group: string[],
                sendingOrg: string[],
                year: number,
                showThai: boolean,
                showNonThai: boolean,
              }>) {
    this.urlBinding.addParam('group', []);
    this.urlBinding.addParam('sendingOrg', []);
    this.urlBinding.addParam('showThai', true);
    this.urlBinding.addParam('showNonThai', true);
    this.urlBinding.addParam('year', Year.CurrentSchoolYear(), true);
    this.urlBinding.addParam('search', '').subscribe(value => this.dataSource.filter = value.trim().toUpperCase());
    this.urlBinding.onParamsUpdated = values => this.dataSource.filterUpdated();
    this.dataSource.filterPredicate = ((data, filter) =>
        data.person.firstName.toUpperCase().startsWith(filter)
        || data.person.lastName.toUpperCase().startsWith(filter)
        || data.person.preferredName.toUpperCase().startsWith(filter)
    );
    this.dataSource.customFilter = value => {
      if (!this.urlBinding.values.showThai && value.person.isThai) return false;
      if (!this.urlBinding.values.showNonThai && !value.person.isThai) return false;
      if (this.urlBinding.values.group.length > 0 && !this.urlBinding.values.group.includes(value.person.staff.orgGroupName)) return false;
      if (this.urlBinding.values.sendingOrg.length > 0 && !this.urlBinding.values.sendingOrg.includes(value.person.staff.missionOrgName)) return false;

      return true;
    };
    this.route.data.subscribe((value: { people: PersonAndLeaveDetails[] }) => {
      this.dataSource.unfilteredData = value.people.map(p => {
        let plm = new PersonLeaveModel();
        plm.person = p.person;
        for (let leave of p.leaveUsages) plm.appendLeave(leave);
        return plm;
      });
      if (!this.urlBinding.loadFromParams()) this.dataSource.filterUpdated();
      this.route.url.subscribe(url => {
        this.isSupervisor = url.some(segment => segment.path == 'supervisor');
      });

      //filter list to distinct
      this.allOrgGroups = value.people
        .map(v => v.person.staff.orgGroupName)
        .filter((v, index, array) => array.indexOf(v) == index && v != null)
        .sort();
      this.allMissionOrgs = value.people
        .map(v => v.person.staff.missionOrgName)
        .filter((v, index, array) => array.indexOf(v) == index && v != null)
        .sort();
    });
  }

  ngOnInit() {
    this.dataSource.sort = this.sort;
  }
}

