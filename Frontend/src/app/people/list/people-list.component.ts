import { ChangeDetectionStrategy, Component, OnInit, ViewChild } from '@angular/core';
import { Person } from '../person';
import { ActivatedRoute } from '@angular/router';
import { AppDataSource } from '../../classes/app-data-source';
import { MatSort } from '@angular/material/sort';
import { UrlBindingService } from '../../services/url-binding.service';

@Component({
  selector: 'app-people-list',
  templateUrl: './people-list.component.html',
  styleUrls: ['./people-list.component.scss'],
  providers: [UrlBindingService],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PeopleListComponent implements OnInit {
  public dataSource: AppDataSource<Person>;
  public columns = ['preferredName', 'lastName', 'staffId', 'isSchoolAid', 'donorId', 'isParent', 'isAlumni'];
  @ViewChild(MatSort) sort: MatSort;

  get title() {
    return this.route.snapshot.data.title || 'People';
  }

  constructor(private route: ActivatedRoute, public urlBinding: UrlBindingService<{ search: string }>) {
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<Person>();
    this.dataSource.customColumnAccessor('staffId', data => <any>!!data.staffId);
    this.urlBinding.addParam('search', '').subscribe(value => this.dataSource.filter = value.toUpperCase());
    this.dataSource.sort = this.sort;
    this.dataSource.bindToRouteData(this.route, 'people');
    this.dataSource.filterPredicate = ((data, filter) =>
      (data.firstName || '').toUpperCase().startsWith(filter)
      || (data.lastName || '').toUpperCase().startsWith(filter)
      || (data.preferredName || '').toUpperCase().startsWith(filter));
  }

  applyFilter(filterValue: string) {
    this.dataSource.filter = filterValue.trim().toUpperCase();
  }
}
