import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LeaveRequest, LeaveRequestWithNames } from './leave-request';
import { Person } from '../person';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class LeaveRequestService {

  constructor(private http: HttpClient) {
  }

  requestLeave(leaveRequest: LeaveRequest): Promise<Object> {
    return this.http.post<Person>('/api/leaverequest/', leaveRequest).toPromise();
  }

  list(): Observable<LeaveRequestWithNames[]> {
    return this.http.get<LeaveRequestWithNames[]>('/api/leaveRequest');
  }

}
