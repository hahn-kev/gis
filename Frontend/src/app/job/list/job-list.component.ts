import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../classes/app-data-source';
import { ActivatedRoute } from '@angular/router';
import {
  AllJobTypes,
  JobStatus,
  jobStatusName,
  JobType,
  jobTypeName,
  JobWithFilledInfo,
  NonSchoolAidJobStatus
} from '../job';
import { MatSort } from '@angular/material';
import { UrlBindingService } from '../../services/url-binding.service';

@Component({
  selector: 'app-job-list',
  templateUrl: './job-list.component.html',
  styleUrls: ['./job-list.component.scss'],
  providers: [UrlBindingService]
})
export class JobListComponent implements OnInit {
  public jobStatus = Object.keys(JobStatus);
  public jobTypes = Object.keys(JobType);
  public dataSource: AppDataSource<JobWithFilledInfo>;
  public statusName = jobStatusName;
  public typeName = jobTypeName;
  @ViewChild(MatSort) sort: MatSort;

  constructor(private route: ActivatedRoute,
              public urlBinding: UrlBindingService<{ search: string, showOnlyOpen: boolean, status: JobStatus[], type: JobType[] }>) {
    this.dataSource = new AppDataSource<JobWithFilledInfo>();
    this.dataSource.bindToRouteData(this.route, 'jobs');
    this.dataSource.customFilter = (row: JobWithFilledInfo) => {
      const values = this.urlBinding.values;
      if (values.showOnlyOpen && row.open <= 0) return false;
      if (values.status.length != this.jobStatus.length && !values.status.includes(row.status)) return false;
      if (values.type.length != this.jobTypes.length && !values.type.includes(row.type)) return false;
      return true;
    };
    this.dataSource.filterPredicate = ((data, filter) =>
        (data.title || '').toUpperCase().includes(filter)
        || (data.jobDescription || '').toUpperCase().includes(filter)
        || (data.orgGroupName || '').toUpperCase().includes(filter)
    );

    this.urlBinding.addParam('search', '').subscribe(value => this.dataSource.filter = value.toUpperCase());
    this.urlBinding.addParam('showOnlyOpen', false);
    this.urlBinding.addParam('status', NonSchoolAidJobStatus);
    this.urlBinding.addParam('type', AllJobTypes);
    this.urlBinding.onParamsUpdated = values => this.dataSource.filterUpdated();
    //load returns true if params updated
    if (!this.urlBinding.loadFromParams()) this.dataSource.filterUpdated();
  }

  ngOnInit(): void {
    this.dataSource.sort = this.sort;
  }

  jobSelectLabel(types: JobStatus[]) {
    if (types.length == this.jobStatus.length) return 'All';
    if (this.areListsEqual(types, NonSchoolAidJobStatus)) return 'Staff Jobs';
    return types.map(value => this.statusName(value)).join(', ');
  }

  jobTypeSelectLabel(status: JobType[]) {
    if (status.length === this.jobTypes.length) return 'All';
    return status.map(value => this.typeName(value)).join(', ');
  }

  areListsEqual<T>(a: T[], b: T[]) {
    if (a === b) return true;
    if (a == null || b == null) return false;
    if (a.length != b.length) return false;

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
