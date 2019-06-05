import { Component, OnInit, ViewChild } from '@angular/core';
import { PersonWithRoleSummaries } from '../person';
import { ActivatedRoute } from '@angular/router';
import { AppDataSource } from '../../classes/app-data-source';
import { MatSort } from '@angular/material/sort';
import { UrlBindingService } from '../../services/url-binding.service';

@Component({
  selector: 'app-school-aid-list',
  templateUrl: './school-aid-list.component.html',
  styleUrls: ['./school-aid-list.component.scss'],
  providers: [UrlBindingService]
})
export class SchoolAidListComponent implements OnInit {
  public dataSource: AppDataSource<PersonWithRoleSummaries>;
  public columns = ['preferredName', 'lastName', 'isActive'];
  @ViewChild(MatSort, {static: true}) sort: MatSort;

  constructor(private route: ActivatedRoute, public urlBinding: UrlBindingService<{ search: string }>) {
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<PersonWithRoleSummaries>();
    this.urlBinding.addParam('search', '')
      .subscribe(value => this.dataSource.filter = value.toUpperCase());
    this.dataSource.sort = this.sort;
    this.dataSource.bindToRouteData(this.route, 'people');
    this.dataSource.filterPredicate = ((data, filter) =>
      (data.firstName || '').toUpperCase().startsWith(filter)
      || (data.lastName || '').toUpperCase().startsWith(filter)
      || (data.preferredName || '').toUpperCase().startsWith(filter));
      this.dataSource.connect().subscribe((data) => {
          this.dataUpdated(data);
      });
  }

  applyFilter(filterValue: string) {
    this.dataSource.filter = filterValue.trim().toUpperCase();
  }

  activeCount = 0;
  inactiveCount = 0;
  dataUpdated(data: PersonWithRoleSummaries[]) {
    this.activeCount = 0;
    this.inactiveCount = 0;
    for (let person of data) {
      this.activeCount += person.isActive ? 1 : 0;
      this.inactiveCount += !person.isActive ? 1 : 0;
    }
  }
}
