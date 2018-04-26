import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RoleWithJob } from '../role';
import { AppDataSource } from '../../classes/app-data-source';
import { MatSort } from '@angular/material';
import { JobStatus, jobStatusName as jobTypeName, NonSchoolAidJobTypes } from '../../job/job';
import { Year } from '../training-requirement/year';
import { UrlBindingService } from '../../services/url-binding.service';

@Component({
  selector: 'app-roles-report',
  templateUrl: './roles-report.component.html',
  styleUrls: ['./roles-report.component.scss'],
  providers: [UrlBindingService]
})
export class RolesReportComponent implements OnInit {
  public dataSource: AppDataSource<RoleWithJob>;
  public typeName = jobTypeName;
  public jobTypes = Object.keys(JobStatus);
  public schoolYears = Year.years();
  @ViewChild(MatSort) sort: MatSort;


  constructor(private route: ActivatedRoute,
              private router: Router,
              public urlBinding: UrlBindingService<{ year: number, type: JobStatus[], start: string, search: string }>) {
    this.dataSource = new AppDataSource<RoleWithJob>();

    this.dataSource.bindToRouteData(this.route, 'roles');
    this.urlBinding.addParam('year', Year.CurrentSchoolYear(), true);
    this.urlBinding.addParam('type', NonSchoolAidJobTypes);
    this.urlBinding.addParam('search', '').subscribe(value => this.dataSource.filter = value.toUpperCase());
    // this.route.params.subscribe((params: { start }) => this.during = params.start === 'during');
    // this.route.queryParams.subscribe((params: { begin, end }) => {
    //   this.beginDate = this.parseDate(params.begin, Date.now() - this.oneYearInMs / 2);
    //   this.endDate = this.parseDate(params.end, Date.now() + this.oneYearInMs / 2);
    // });
    this.dataSource.filterPredicate = (data, filter) => {
      return data.preferredName.toUpperCase().startsWith(filter)
        || data.lastName.toUpperCase().startsWith(filter)
        || data.job.title.toUpperCase().includes(filter)
        || (data.job.orgGroup && data.job.orgGroup.groupName.toUpperCase().includes(filter));
    };
    this.dataSource.customFilter = value => {
      if (!this.urlBinding.values.type.includes(value.job.status)) return false;
      return true;
    };
    this.urlBinding.onParamsUpdated = values => this.dataSource.filterUpdated();
    //load returns true if params updated
    if (!this.urlBinding.loadFromParams()) this.dataSource.filterUpdated();
  }

  ngOnInit(): void {
    this.dataSource.sort = this.sort;
  }

  jobSelectLabel(types: JobStatus[]) {
    if (typeof types === 'string') return types;
    if (types.length === this.jobTypes.length) return 'All';
    if (this.areListsEqual(types, NonSchoolAidJobTypes)) return 'Staff Jobs';
    return types.map(value => this.typeName(value)).join(', ');
  }

  areListsEqual(a: JobStatus[], b: JobStatus[]) {
    if (a === b) return true;
    if (a == null || b == null) return false;
    if (a.length !== b.length) return false;

    // If you don't care about the order of the elements inside
    // the array, you should sort both arrays here.
    a.sort();
    b.sort();

    for (let i = 0; i < a.length; ++i) {
      if (a[i] !== b[i]) return false;
    }
    return true;
  }
}
