import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {LeaveRequest, LeaveRequestWithNames} from './leave-request';
import {Person} from '../person';
import {Observable, of} from 'rxjs';
import {PersonAndLeaveDetails} from './person-and-leave-details';
import {LeaveUsage} from '../self/self';
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
    if (!personId) return of([]);
    return this.http.get<LeaveRequestWithNames[]>('/api/leaveRequest/person/' + personId);
  }

  listForSupervisor(supervisorId: string) {
    return this.http.get<LeaveRequestWithNames[]>('/api/leaveRequest/supervisor/' + supervisorId);
  }

  listPeopleWithLeave(): Observable<PersonAndLeaveDetails[]> {
    return this.http.get<PersonAndLeaveDetails[]>('/api/leaveRequest/people');
  }

  listMyPeopleWithLeave() {
    return this.http.get<PersonAndLeaveDetails[]>('/api/leaveRequest/people/mine');
  }

  isOverUsingLeave(leaveRequest: LeaveRequest, leaveUsages: LeaveUsage[]) {
    const leaveUsage = leaveUsages.find(value => value.leaveType == leaveRequest.type);
    if (leaveUsage == null) return true;
    if (leaveUsage.left <= 1) return true;
    return leaveUsage.left - leaveRequest.days < 0;
  }

  weekDays(leaveRequest: LeaveRequest) {
    return this.weekDaysBetween(leaveRequest.startDate, leaveRequest.endDate);
  }

  weekDaysBetween(dayOne: Date | string, dayTwo: Date | string): number {
    //weekDays calc is start inclusive end exclusive so we need to add a day to the end
    const momentTwo = moment(dayTwo).startOf('day').add(1, 'd');
    return buisness.weekDays(moment(dayOne).startOf('day'), momentTwo);
  }
}
