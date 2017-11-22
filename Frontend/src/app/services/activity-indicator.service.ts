import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

@Injectable()
export class ActivityIndicatorService {
  private indicatorSubject = new BehaviorSubject<boolean>(false);
  constructor() { }

  public observeIndicator() {
    return this.indicatorSubject.asObservable();
  }

  public showIndicator() {
    this.indicatorSubject.next(true);
  }
  public hideIndicator() {
    this.indicatorSubject.next(false);
  }
}
