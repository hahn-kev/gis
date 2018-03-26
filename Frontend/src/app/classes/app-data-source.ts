import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { ActivatedRoute } from '@angular/router';
import { MatTableDataSource } from '@angular/material';

export class AppDataSource<T> extends MatTableDataSource<T> {
  private customColumnAccessors: { [key: string]: (data: T) => string | number } = {};

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

  bindToRouteData(route: ActivatedRoute, dataName: string): void {
    route.data.subscribe((value) => {
      this.data = value[dataName];
    });
  }
}
