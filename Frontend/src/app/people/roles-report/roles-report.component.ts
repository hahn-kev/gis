import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RoleWithJob } from '../role';
import { AppDataSource } from '../../classes/app-data-source';
import { MatSort } from '@angular/material';
import { AllJobTypes, JobStatus, jobStatusName, JobType, jobTypeName, NonSchoolAidJobStatus } from '../../job/job';
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
  public statusName = jobStatusName;
  public jobStatus = Object.keys(JobStatus);
  public jobTypes = Object.keys(JobType);
  public schoolYears = Year.years();
  @ViewChild(MatSort) sort: MatSort;


  constructor(private route: ActivatedRoute,
              private router: Router,
              public urlBinding: UrlBindingService<{ year: number, type: JobType[], status: JobStatus[], start: string, search: string }>) {
    this.dataSource = new AppDataSource<RoleWithJob>();

    this.dataSource.bindToRouteData(this.route, 'roles');
    this.urlBinding.addParam('year', Year.CurrentSchoolYear(), true);
    this.urlBinding.addParam('type', AllJobTypes);
    this.urlBinding.addParam('status', NonSchoolAidJobStatus);
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
      const values = this.urlBinding.values;
      if (values.status.length != this.jobStatus.length && !values.status.includes(value.job.status)) return false;
      if (values.type.length != this.jobTypes.length && !values.type.includes(value.job.type)) return false;
      return true;
    };
    this.urlBinding.onParamsUpdated = values => this.dataSource.filterUpdated();
    //load returns true if params updated
    if (!this.urlBinding.loadFromParams()) this.dataSource.filterUpdated();
  }

  ngOnInit(): void {
    this.dataSource.sort = this.sort;
  }

  jobSelectLabel(status: JobStatus[]) {
    if (typeof status === 'string') return status;
    if (status.length === this.jobStatus.length) return 'All';
    if (this.areListsEqual(status, NonSchoolAidJobStatus)) return 'Staff Jobs';
    return status.map(value => this.statusName(value)).join(', ');
  }

  jobTypeSelectLabel(status: JobType[]) {
    if (typeof status === 'string') return status;
    if (status.length === this.jobTypes.length) return 'All';
    return status.map(value => this.typeName(value)).join(', ');
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
