import { CollectionViewer, DataSource } from '@angular/cdk/collections';
import { Person } from '../people/person';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Observable } from 'rxjs/Observable';
import { ActivatedRoute } from '@angular/router';

export class AppDataSource<T> extends DataSource<T> {
  public ObserverData = new BehaviorSubject<T[]>([]);

  constructor() {
    super();
  }

  bindToRouteData(route: ActivatedRoute, dataName: string): void {
    route.data.subscribe((value) => {
      this.ObserverData.next(value[dataName]);
    });
  }

  connect(collectionViewer: CollectionViewer): Observable<T[]> {
    return this.ObserverData.asObservable();
  }

  disconnect(collectionViewer: CollectionViewer): void {
  }
}
