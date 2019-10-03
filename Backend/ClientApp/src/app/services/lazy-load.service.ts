import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';

@Injectable()
export class LazyLoadService {

  private observables = new Map<string, Observable<any>>();

  constructor() {
  }

  share<T>(key: string, action: () => Observable<T>): Observable<T> {
    if (this.observables.has(key)) return this.observables.get(key);
    let shared = action()
      .pipe(
      shareReplay()
    );

    this.observables.set(key, shared);
    return shared;
  }
}
