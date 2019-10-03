import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../../classes/app-data-source';
import { OrgGroupWithSupervisor } from '../org-group';
import { ActivatedRoute } from '@angular/router';
import { UrlBindingService } from '../../../services/url-binding.service';
import { MatSort } from '@angular/material/sort';

@Component({
  selector: 'app-org-group-list',
  templateUrl: './org-group-list.component.html',
  styleUrls: ['./org-group-list.component.scss'],
  providers: [UrlBindingService]
})
export class OrgGroupListComponent implements OnInit {
  columns = ['groupName', 'parent', 'supervisor', 'type'];
  public dataSource: AppDataSource<OrgGroupWithSupervisor>;
  @ViewChild(MatSort, {static: true}) sort: MatSort;

  supervisorCountById: { [supervisorId: string]: number } = {};
  groupNameById: { [groupId: string]: string } = {};


  constructor(private route: ActivatedRoute, public urlBinding: UrlBindingService<{ search: string }>) {
    this.urlBinding.addParam('search', '');
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<OrgGroupWithSupervisor>();
    this.dataSource.sort = this.sort;
    this.dataSource.bindToRouteData(this.route, 'groups');
    this.dataSource.filterPredicate = (data, filter) => data.groupName.toUpperCase().includes(filter);
    this.urlBinding.observableValues.search.subscribe(value => this.dataSource.filter = value.toUpperCase());
    this.urlBinding.loadFromParams();
    this.dataSource.customColumnAccessor('supervisor',
      data => !data.supervisorPerson ?
        '' :
        (data.supervisorPerson.preferredName || data.supervisorPerson.firstName) + ' ' + data.supervisorPerson.lastName);
    this.dataSource.customColumnAccessor('parent', data => this.groupNameById[data.id]);

    for (let group of this.dataSource.unfilteredData) {
      this.groupNameById[group.id] = group.groupName;
      if (!group.supervisor) continue;
      this.supervisorCountById[group.supervisor] = (this.supervisorCountById[group.supervisor] || 0) + 1;
    }
  }

}
