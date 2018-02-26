import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { ActivatedRoute } from '@angular/router';
import { MatTableDataSource } from '@angular/material';

export class AppDataSource<T> extends MatTableDataSource<T> {
  public ObserverData = new BehaviorSubject<T[]>([]);

  constructor() {
    super();
  }

  bindToRouteData(route: ActivatedRoute, dataName: string): void {
    route.data.subscribe((value) => {
      this.data = value[dataName];
    });
  }
}
