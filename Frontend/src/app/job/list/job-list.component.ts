import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../classes/app-data-source';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Job, JobType, jobTypeName, JobWithFilledInfo, NonSchoolAidJobTypes } from '../job';
import { MatSort } from '@angular/material';

@Component({
  selector: 'app-job-list',
  templateUrl: './job-list.component.html',
  styleUrls: ['./job-list.component.scss']
})
export class JobListComponent implements OnInit {
  public jobTypes = Object.keys(JobType);
  public dataSource: AppDataSource<JobWithFilledInfo>;
  public typeName = jobTypeName;
  public showOnlyOpen = false;
  public shownTypes: JobType[] = [];
  @ViewChild(MatSort) sort: MatSort;

  constructor(private route: ActivatedRoute, private router: Router) {

  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<JobWithFilledInfo>();
    this.dataSource.sort = this.sort;
    this.dataSource.bindToRouteData(this.route, 'jobs');
    this.dataSource.customFilter = (row: JobWithFilledInfo) => {
      if (this.showOnlyOpen && row.open <= 0) return false;
      if (!this.shownTypes.includes(row.type)) return false;
      return true;
    };
    this.dataSource.filterPredicate = ((data, filter) =>
        (data.title || '').toUpperCase().includes(filter)
        || (data.jobDescription || '').toUpperCase().includes(filter)
        || (data.orgGroupName || '').toUpperCase().includes(filter)
    );
    this.route.queryParamMap.subscribe(params => {
      this.showOnlyOpen = params.has('onlyOpen') ? params.get('onlyOpen') == 'true' : false;
      this.shownTypes = (params.has('types') ? params.getAll('types') : this.jobTypes)
        .map(jt => JobType[jt]);
      let oldFilter = this.dataSource.filter;
      this.dataSource.filter = (params.get('filter') || '').toUpperCase();
      this.dataSource.filterUpdated();
    });
  }


  applyFilter(filterValue: string) {
    this.setQuery(filterValue, this.showOnlyOpen, this.shownTypes);
  }

  applyShowOnlyOpen(onlyOpen: boolean) {
    this.setQuery(this.dataSource.filter, onlyOpen, this.shownTypes);
  }

  applyShownTypes(types: JobType[]) {
    this.setQuery(this.dataSource.filter, this.showOnlyOpen, types);
  }

  setQuery(filter: string, onlyOpen: boolean, types: JobType[]) {
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

  jobSelectLabel(types: JobType[]) {

    if (types.length == this.jobTypes.length) return 'All';
    if (this.areListsEqual(types, NonSchoolAidJobTypes)) return 'Staff Jobs';
    return types.join(', ');
  }

  areListsEqual(a: JobType[], b: JobType[]) {
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
