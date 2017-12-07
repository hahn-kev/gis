import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LeaveRequest } from './leave-request';
import { Person } from '../person';

@Injectable()
export class LeaveRequestService {

  constructor(private http: HttpClient) {
  }

  requestLeave(leaveRequest: LeaveRequest) {
    return this.http.post<Person>('/api/leaverequest/', leaveRequest).toPromise();
  }

}
