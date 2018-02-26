import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LeaveRequest, LeaveRequestWithNames } from './leave-request';
import { Person } from '../person';
import { Observable } from 'rxjs/Observable';
import { PersonAndLeaveDetails } from './person-and-leave-details';
import { LeaveUseage } from '../self/self';

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

  deleteRequest(id: string): Observable<string> {
    return this.http.delete('/api/leaveRequest/' + id, {responseType: 'text'});
  }

  updateLeave(leaveRequest: LeaveRequest): Observable<string> {
    return this.http.put('/api/leaveRequest/', leaveRequest, {responseType: 'text'});
  }

  list(): Observable<LeaveRequestWithNames[]> {
    return this.http.get<LeaveRequestWithNames[]>('/api/leaveRequest');
  }

  listByPersonId(personId: string): Observable<LeaveRequestWithNames[]> {
    return this.http.get<LeaveRequestWithNames[]>('/api/leaveRequest/person/' + personId);
  }

  listPeopleWithLeave(listAll: boolean) {
    return this.http.get<PersonAndLeaveDetails[]>('/api/leaveRequest/people',
      {params: {listAll: listAll ? 'true' : 'false'}});
  }

  isOverUsingLeave(leaveRequest: LeaveRequest, leaveUseages: LeaveUseage[]) {
    let leaveUsage = leaveUseages.find(value => value.leaveType == leaveRequest.type);
    if (leaveUsage.left <= 1) return true;
    const oneDayMs = 24 * 60 * 60 * 1000;
    let daysOfLeave = Math.round(Math.abs((leaveRequest.startDate.getTime() - leaveRequest.endDate.getTime()) / (oneDayMs)));
    return leaveUsage.left - daysOfLeave < 0;
  }
}
