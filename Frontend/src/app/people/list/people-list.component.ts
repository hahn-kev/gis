import { Component, OnInit, ViewChild } from '@angular/core';
import { Person } from '../person';
import { ActivatedRoute } from '@angular/router';
import { AppDataSource } from '../../classes/app-data-source';
import { MatSort } from '@angular/material';

@Component({
  selector: 'app-people-list',
  templateUrl: './people-list.component.html',
  styleUrls: ['./people-list.component.scss']
})
export class PeopleListComponent implements OnInit {
  public dataSource: AppDataSource<Person>;
  @ViewChild(MatSort) sort: MatSort;
  get title() {
    return this.route.snapshot.data.title || 'People';
  }

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<Person>();
    this.dataSource.sort = this.sort;
    this.dataSource.bindToRouteData(this.route, 'people');
    this.dataSource.filterPredicate = ((data, filter) =>
      data.firstName.toUpperCase().startsWith(filter) || data.lastName.toUpperCase().startsWith(filter));
  }

  applyFilter(filterValue: string) {
    this.dataSource.filter = filterValue.trim().toUpperCase();
  }
}
