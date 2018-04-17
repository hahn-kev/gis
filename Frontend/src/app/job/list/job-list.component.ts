import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../classes/app-data-source';
import { ActivatedRoute } from '@angular/router';
import { Job, JobWithFilledInfo } from '../job';
import { MatSort } from '@angular/material';

@Component({
  selector: 'app-job-list',
  templateUrl: './job-list.component.html',
  styleUrls: ['./job-list.component.scss']
})
export class JobListComponent implements OnInit {
  public dataSource: AppDataSource<JobWithFilledInfo>;
  public typeName = Job.typeName;
  public showOnlyOpen = false;
  @ViewChild(MatSort) sort: MatSort;

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<JobWithFilledInfo>();
    this.dataSource.sort = this.sort;
    this.dataSource.bindToRouteData(this.route, 'jobs');
    this.dataSource.customFilter = row => this.showOnlyOpen ? row.open > 0 : true;
    this.dataSource.filterPredicate = ((data, filter) =>
      (data.title || '').toUpperCase().includes(filter)
      || (data.jobDescription || '').toUpperCase().includes(filter)
      || (data.orgGroupName || '').toUpperCase().includes(filter)
    );
  }


  applyFilter(filterValue: string) {
    this.dataSource.filter = filterValue.trim().toUpperCase();
  }

}
