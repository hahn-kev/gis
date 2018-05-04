import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { PersonWithOthers } from '../person';

@Injectable()
export class SelfService implements Resolve<PersonWithOthers> {
  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<PersonWithOthers> | Promise<PersonWithOthers> | PersonWithOthers {
    return this.http.get<PersonWithOthers>('/api/self/');
  }

  constructor(private http: HttpClient) {
  }
}
