import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LeaveRequest, LeaveRequestWithNames } from './leave-request';
import { Person } from '../person';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class LeaveRequestService {

  constructor(private http: HttpClient) {
  }

  requestLeave(leaveRequest: LeaveRequest): Promise<Person> {
    return this.http.post<Person>('/api/leaverequest/', leaveRequest).toPromise();
  }

  getById(id: string): Observable<LeaveRequestWithNames> {
    return this.http.get<LeaveRequestWithNames>('/api/leaveRequest/' + id);
  }

  updateLeave(leaveRequest: LeaveRequest): Observable<string> {
    return this.http.put('/api/leaveRequest/', leaveRequest, {responseType: 'text'});
  }

  list(): Observable<LeaveRequestWithNames[]> {
    return this.http.get<LeaveRequestWithNames[]>('/api/leaveRequest');
  }

}
