import { Component, OnInit } from '@angular/core';
import { AppDataSource } from '../../classes/app-data-source';
import { ActivatedRoute } from '@angular/router';
import { MissionOrg } from '../mission-org';

@Component({
  selector: 'app-mission-org-list',
  templateUrl: './mission-org-list.component.html',
  styleUrls: ['./mission-org-list.component.scss']
})
export class MissionOrgListComponent implements OnInit {
  public dataSource: AppDataSource<MissionOrg>;

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<MissionOrg>();
    this.dataSource.bindToRouteData(this.route, 'missionOrgs');
    this.dataSource.filterPredicate = ((data, filter) => data.name.toUpperCase().startsWith(filter));
  }


  applyFilter(filterValue: string) {
    this.dataSource.filter = filterValue.trim().toUpperCase();
  }
}
