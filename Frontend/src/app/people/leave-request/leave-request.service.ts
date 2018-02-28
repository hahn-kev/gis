import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LeaveRequest, LeaveRequestWithNames } from './leave-request';
import { Person } from '../person';
import { Observable } from 'rxjs/Observable';
import { PersonAndLeaveDetails } from './person-and-leave-details';
import { LeaveUseage } from '../self/self';
import * as moment from 'moment';
import * as buisness from 'moment-business';

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

  listPeopleWithLeave(listAll: boolean): Observable<PersonAndLeaveDetails[]> {
    return this.http.get<PersonAndLeaveDetails[]>('/api/leaveRequest/people',
      {params: {listAll: listAll ? 'true' : 'false'}});
  }

  isOverUsingLeave(leaveRequest: LeaveRequest, leaveUseages: LeaveUseage[]) {
    let leaveUsage = leaveUseages.find(value => value.leaveType == leaveRequest.type);
    if (leaveUsage.left <= 1) return true;
    let daysOfLeave = this.weekDaysBetween(leaveRequest.startDate, leaveRequest.endDate);
    return leaveUsage.left - daysOfLeave < 0;
  }

  weekDays(leaveRequest: LeaveRequest) {
    return this.weekDaysBetween(leaveRequest.startDate, leaveRequest.endDate);
  }

  weekDaysBetween(dayOne: Date | string, dayTwo: Date | string): number {
    //weekDays calc is start inclusive end exclusive so we need to add a day to the end
    let momentTwo = moment(dayTwo).startOf('day').add(1, 'd');
    return buisness.weekDays(moment(dayOne).startOf('day'), momentTwo);
  }
}
