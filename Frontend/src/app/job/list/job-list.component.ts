import { Component, OnInit } from '@angular/core';
import { AppDataSource } from '../../classes/app-data-source';
import { ActivatedRoute } from '@angular/router';
import { Job } from '../job';

@Component({
  selector: 'app-job-list',
  templateUrl: './job-list.component.html',
  styleUrls: ['./job-list.component.scss']
})
export class JobListComponent implements OnInit {
  public dataSource: AppDataSource<Job>;

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<Job>();
    this.dataSource.bindToRouteData(this.route, 'jobs');
  }


}
