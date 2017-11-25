import { Component, OnInit } from '@angular/core';
import { CollectionViewer, DataSource } from '@angular/cdk/collections';
import { Person } from '../person';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-people-list',
  templateUrl: './people-list.component.html',
  styleUrls: ['./people-list.component.scss']
})
export class PeopleListComponent implements OnInit {
  public dataSource: PeopleDataSource;

  constructor(private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.dataSource = new PeopleDataSource();
    this.route.data.subscribe((value: { people: Person[] }) => {
      this.dataSource.ObserverData.next(value.people);
    });
  }

}


class PeopleDataSource extends DataSource<Person> {
  public ObserverData = new BehaviorSubject<Person[]>([]);

  constructor() {
    super();
  }

  connect(collectionViewer: CollectionViewer): Observable<Person[]> {
    return this.ObserverData.asObservable();
  }

  disconnect(collectionViewer: CollectionViewer): void {
  }
}
