import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../classes/app-data-source';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Job, JobStatus, jobStatusName, JobWithFilledInfo, NonSchoolAidJobStatus } from '../job';
import { MatSort } from '@angular/material';

@Component({
  selector: 'app-job-list',
  templateUrl: './job-list.component.html',
  styleUrls: ['./job-list.component.scss']
})
export class JobListComponent implements OnInit {
  public jobStatus = Object.keys(JobStatus);
  public dataSource: AppDataSource<JobWithFilledInfo>;
  public statusName = jobStatusName;
  public filter: string;
  public showOnlyOpen = false;
  public shownStatus: JobStatus[] = [];
  @ViewChild(MatSort) sort: MatSort;

  constructor(private route: ActivatedRoute, private router: Router) {

  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<JobWithFilledInfo>();
    this.dataSource.sort = this.sort;
    this.dataSource.bindToRouteData(this.route, 'jobs');
    this.dataSource.customFilter = (row: JobWithFilledInfo) => {
      if (this.showOnlyOpen && row.open <= 0) return false;
      if (!this.shownStatus.includes(row.status)) return false;
      return true;
    };
    this.dataSource.filterPredicate = ((data, filter) =>
        (data.title || '').toUpperCase().includes(filter)
        || (data.jobDescription || '').toUpperCase().includes(filter)
        || (data.orgGroupName || '').toUpperCase().includes(filter)
    );
    this.route.queryParamMap.subscribe(params => {
      this.showOnlyOpen = params.has('onlyOpen') ? params.get('onlyOpen') == 'true' : false;
      this.shownStatus = (params.has('status') ? params.getAll('status') : this.jobStatus)
        .map(jt => JobStatus[jt]);
      let oldFilter = this.dataSource.filter;
      this.filter = params.get('filter');
      this.dataSource.filter = (this.filter || '').toUpperCase();
      this.dataSource.filterUpdated();
    });
  }


  applyFilter(filterValue: string) {
    this.setQuery(filterValue, this.showOnlyOpen, this.shownStatus);
  }

  applyShowOnlyOpen(onlyOpen: boolean) {
    this.setQuery(this.dataSource.filter, onlyOpen, this.shownStatus);
  }

  applyShownTypes(types: JobStatus[]) {
    this.setQuery(this.dataSource.filter, this.showOnlyOpen, types);
  }

  setQuery(filter: string, onlyOpen: boolean, types: JobStatus[]) {
    let params: Params = {
      onlyOpen: onlyOpen,
      types: types
    };
    if (filter !== '') params.filter = filter;
    this.router.navigate(['.'], {
      relativeTo: this.route,
      queryParams: params
    });
  }

  jobSelectLabel(types: JobStatus[]) {

    if (types.length == this.jobStatus.length) return 'All';
    if (this.areListsEqual(types, NonSchoolAidJobStatus)) return 'Staff Jobs';
    return types.map(value => this.statusName(value)).join(', ');
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
