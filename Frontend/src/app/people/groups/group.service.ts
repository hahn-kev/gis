import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { OrgGroup } from './org-group';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class GroupService {

  constructor(private http: HttpClient) {
  }

  getGroup(id: string): Observable<Object> {
    return this.http.get<OrgGroup>('/api/orggroup/' + id);
  }

  getAll(): Observable<Object> {
    return this.http.get<OrgGroup[]>('/api/orggroup');
  }

  updateGroup(group: OrgGroup): Promise<Object> {
    return this.http.post('/api/orggroup', group).toPromise();
  }
}
