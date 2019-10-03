import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../classes/app-data-source';
import { ActivatedRoute } from '@angular/router';
import { AllJobTypes, JobStatus, JobType, JobWithFilledInfo, NonSchoolAidJobStatus } from '../job';
import { MatSort } from '@angular/material/sort';
import { UrlBindingService } from '../../services/url-binding.service';
import { JobStatusNamePipe } from '../job-status-name.pipe';
import { JobTypeNamePipe } from '../job-type-name.pipe';

@Component({
  selector: 'app-job-list',
  templateUrl: './job-list.component.html',
  styleUrls: ['./job-list.component.scss'],
  providers: [UrlBindingService, JobStatusNamePipe, JobTypeNamePipe]
})
export class JobListComponent implements OnInit {
  public jobStatus = Object.keys(JobStatus);
  public jobTypes = Object.keys(JobType);
  public dataSource: AppDataSource<JobWithFilledInfo>;
  public allOrgGroups: string[] = [];
  @ViewChild(MatSort, {static: true}) sort: MatSort;

  public columns = ['title', 'status', 'type', 'gradeNo', 'orgGroupName', 'current', 'positions', 'filled', 'open'];

  constructor(private route: ActivatedRoute,
              public urlBinding: UrlBindingService<{
                search: string,
                showOnlyOpen: boolean,
                status: JobStatus[],
                type: JobType[],
                showInactive: boolean,
                group: string[]
              }>,
              private jobStatusName: JobStatusNamePipe,
              private jobTypeName: JobTypeNamePipe) {
    this.dataSource = new AppDataSource<JobWithFilledInfo>();
    this.dataSource.bindToRouteData(this.route, 'jobs');
    //filter list to distinct
    this.allOrgGroups = this.dataSource.filteredData
      .map(value => value.orgGroupName)
      .filter((value, index, array) => array.indexOf(value) == index && value != null)
      .sort();

    this.dataSource.customFilter = (row: JobWithFilledInfo) => {
      const values = this.urlBinding.values;
      if (values.showOnlyOpen && row.open <= 0) return false;
      if (!values.showInactive && !row.current) return false;
      if (values.status.length != this.jobStatus.length && !values.status.includes(row.status)) return false;
      if (values.type.length != this.jobTypes.length && !values.type.includes(row.type)) return false;
      if (this.urlBinding.values.group.length > 0 &&
        !this.urlBinding.values.group.includes(row.orgGroupName)) return false;
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
    this.urlBinding.addParam('showInactive', false);
    this.urlBinding.addParam('group', []);
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
    return types.map(value => this.jobStatusName.transform(value)).join(', ');
  }

  jobTypeSelectLabel(status: JobType[]) {
    if (status.length === this.jobTypes.length) return 'All';
    return status.map(value => this.jobTypeName.transform(value)).join(', ');
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
