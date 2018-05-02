import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../classes/app-data-source';
import { ActivatedRoute } from '@angular/router';
import { MissionOrgWithNames } from '../mission-org';
import { MatSort } from '@angular/material';

@Component({
  selector: 'app-mission-org-list',
  templateUrl: './mission-org-list.component.html',
  styleUrls: ['./mission-org-list.component.scss']
})
export class MissionOrgListComponent implements OnInit {
  public dataSource: AppDataSource<MissionOrgWithNames>;
  @ViewChild(MatSort) sort: MatSort;

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<MissionOrgWithNames>();
    this.dataSource.sort = this.sort;
    this.dataSource.bindToRouteData(this.route, 'missionOrgs');
    this.dataSource.filterPredicate = ((data, filter) => data.name.toUpperCase().startsWith(filter));
  }


  applyFilter(filterValue: string) {
    this.dataSource.filter = filterValue.trim().toUpperCase();
  }
}
