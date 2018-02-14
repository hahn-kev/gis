import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Self } from './self';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class SelfService implements Resolve<Self> {
  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<Self> | Promise<Self> | Self {
    return this.getSelf(route.params['id']);
  }

  constructor(private http: HttpClient) {
  }

  getSelf(id?: string) {
    return this.http.get<Self>('/api/self/' + (id || ''));
  }
}
