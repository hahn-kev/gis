import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class SettingsService {
  settings = new BehaviorSubject<any>(null);

  constructor(private http: HttpClient) {
    http.get<any>('/api/settings').subscribe(value => this.settings.next(value));
  }

  getAsync<T>(name: string, defaultValue?: T): Observable<T> {
    return this.settings.map(values => values ? values[name] : defaultValue);
  }

  get<T>(name: string, defaultValue?: T): T {
    return this.settings.value ? this.settings.value[name] : defaultValue;
  }
}
