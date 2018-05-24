import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../classes/app-data-source';
import { ActivatedRoute } from '@angular/router';
import { MissionOrgWithNames } from '../mission-org';
import { MatSort } from '@angular/material';
import { UrlBindingService } from '../../services/url-binding.service';

@Component({
  selector: 'app-mission-org-list',
  templateUrl: './mission-org-list.component.html',
  styleUrls: ['./mission-org-list.component.scss'],
  providers: [UrlBindingService]
})
export class MissionOrgListComponent implements OnInit {
  public dataSource = new AppDataSource<MissionOrgWithNames>();
  @ViewChild(MatSort) sort: MatSort;

  constructor(private route: ActivatedRoute, public urlBinding: UrlBindingService<{ search: string }>) {
    this.urlBinding.addParam('search', '').subscribe(value => this.dataSource.filter = value.trim().toUpperCase());
  }

  ngOnInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.bindToRouteData(this.route, 'missionOrgs');
    this.dataSource.filterPredicate = ((data, filter) => data.name.toUpperCase().includes(filter));
    this.urlBinding.loadFromParams();
  }
}
