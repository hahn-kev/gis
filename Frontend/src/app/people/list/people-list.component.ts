import { Component, OnInit } from '@angular/core';
import { CollectionViewer, DataSource } from '@angular/cdk/collections';
import { Person } from '../person';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { ActivatedRoute } from '@angular/router';
import { AppDataSource } from '../../classes/app-data-source';

@Component({
  selector: 'app-people-list',
  templateUrl: './people-list.component.html',
  styleUrls: ['./people-list.component.scss']
})
export class PeopleListComponent implements OnInit {
  public dataSource: AppDataSource<Person>;

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.dataSource = new AppDataSource<Person>();
    this.dataSource.bindToRouteData(this.route, 'people');
  }

}
