import { Component, OnInit } from '@angular/core';
import { AppDataSource } from '../../../classes/app-data-source';
import { OrgGroup } from '../org-group';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-org-group-list',
  templateUrl: './org-group-list.component.html',
  styleUrls: ['./org-group-list.component.scss']
})
export class OrgGroupListComponent implements OnInit {
  public dataSource: AppDataSource<OrgGroup>;

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<OrgGroup>();
    this.dataSource.bindToRouteData(this.route, 'groups');
  }

}
