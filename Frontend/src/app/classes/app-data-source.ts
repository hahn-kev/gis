import { ActivatedRoute } from '@angular/router';
import { MatTableDataSource } from '@angular/material';
import { Observable } from 'rxjs';

export class AppDataSource<T> extends MatTableDataSource<T> {
  private customColumnAccessors: { [key: string]: (data: T) => string | number } = {};
  private unfilteredData: T[];

  constructor() {
    super();
    let ogSort = this.sortingDataAccessor;
    this.sortingDataAccessor = (data, sortHeaderId) => {
      if (this.customColumnAccessors[sortHeaderId]) return this.customColumnAccessors[sortHeaderId](data);
      let index = 0;
      let paramIndex = sortHeaderId.indexOf('.', index);
      while (paramIndex > -1) {
        data = data[sortHeaderId.substring(index, paramIndex)];
        index = paramIndex + 1;
        paramIndex = sortHeaderId.indexOf('.', index);
        if (data === null || data === undefined) return null;
      }
      if (index > 0) {
        sortHeaderId = sortHeaderId.substring(index);
      }
      return ogSort(data, sortHeaderId);
    };
  }

  customColumnAccessor(columnId: string, accessor: (data: T) => string | number) {
    this.customColumnAccessors[columnId] = accessor;
  }

  observe(observable: Observable<T[]>) {
    return observable.subscribe(value => {
      this.unfilteredData = value;
      this.filterUpdated();
    });
  }

  bindToRouteData(route: ActivatedRoute, dataName: string): void {
    route.data.subscribe((value) => {
      this.unfilteredData = value[dataName];
      this.filterUpdated();
    });
  }

  _customFilter = (data: T) => true;
  set customFilter(value: (value: T) => boolean) {
    this._customFilter = value;
  }

  filterUpdated() {
    this.data = this.unfilteredData.filter(value => this._customFilter(value));
  }
}
