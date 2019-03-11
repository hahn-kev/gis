import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LeaveRequest, LeaveRequestWithNames } from './leave-request';
import { Person } from '../person';
import { Observable, of } from 'rxjs';
import { PersonAndLeaveDetails } from './person-and-leave-details';
import { LeaveUsage } from '../self/self';
import * as moment from 'moment';
import * as buisness from 'moment-business';
import { Holiday } from './holiday';

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

  listMyLeave() {
    return this.http.get<LeaveRequestWithNames[]>('/api/leaveRequest/mine');
  }

  listForSupervisor(supervisorId?: string) {
    if (!supervisorId) {
      return this.http.get<LeaveRequestWithNames[]>('/api/leaveRequest/supervisor');
    }
    return this.http.get<LeaveRequestWithNames[]>('/api/leaveRequest/supervisor/' + supervisorId);
  }

  listPeopleWithLeave(year: number): Observable<PersonAndLeaveDetails[]> {
    return this.http.get<PersonAndLeaveDetails[]>('/api/leaveRequest/people', {params: {year: year.toString()}});
  }

  listMyPeopleWithLeave(year: number) {
    return this.http.get<PersonAndLeaveDetails[]>('/api/leaveRequest/people/supervisor',
      {params: {year: year.toString()}});
  }

  listMyLeaveDetails(year: number) {
    return this.http.get<PersonAndLeaveDetails[]>('/api/leaveRequest/people/mine',
      {params: {year: year.toString()}});
  }

  isOverUsingLeave(leaveRequest: LeaveRequest, leaveUsages: LeaveUsage[]) {
    const leaveUsage = leaveUsages.find(value => value.leaveType == leaveRequest.type);
    if (leaveUsage == null) return true;
    if (leaveUsage.left <= 1) return true;
    return leaveUsage.left - leaveRequest.days < 0;
  }

  weekDays(leaveRequest: LeaveRequest, holidays: Holiday[]) {
    let days = this.weekDaysBetween(leaveRequest.startDate, leaveRequest.endDate);
    for (const holiday of holidays) {
      days -= this.overlapingHolidayDays(leaveRequest, holiday);
    }
    return days;
  }

  overlapingHolidayDays(leaveRequest: LeaveRequest, holiday: Holiday) {
    const startDate = moment(leaveRequest.startDate);
    const endDate = moment(leaveRequest.endDate);
    const holidayStart = moment(holiday.start);
    const holidayEnd = moment(holiday.end);
    if (holidayEnd.isBefore(startDate) || holidayStart.isAfter(endDate)) {
      return 0;
    }
    if (holidayStart.isSame(holidayEnd, 'day')) {
      return holidayStart.isBetween(startDate, endDate, 'day', '[]') ? 1 : 0;
    }
    if (startDate.isSame(endDate, 'day')) {
      return startDate.isBetween(holidayStart, holidayEnd, 'day', '[]') ? 1 : 0;
    }
    if (holidayEnd.isSameOrBefore(endDate) && holidayStart.isSameOrAfter(startDate)) {
      return this.weekDaysBetween(holidayStart, holidayEnd);
    }
    if (holidayEnd.isAfter(endDate) && holidayStart.isAfter(startDate) && holidayStart.isSameOrBefore(endDate)) {
      return this.weekDaysBetween(holidayStart, endDate);
    }
    if (holidayStart.isBefore(startDate) && holidayEnd.isBefore(endDate) && holidayEnd.isSameOrAfter(startDate)) {
      return this.weekDaysBetween(startDate, holidayEnd);
    }
    console.error('Not sure how holiday: ' + holiday.start + ' -> ' + holiday.end + ' overlaps with ' + leaveRequest.endDate + ' -> ' + leaveRequest.startDate);
    return 0;
  }

  weekDaysBetween(dayOne: Date | string | moment.Moment, dayTwo: Date | string | moment.Moment): number {
    //weekDays calc is start inclusive end exclusive so we need to add a day to the end
    const momentTwo = moment(dayTwo).startOf('day').add(1, 'd');
    return buisness.weekDays(moment(dayOne).startOf('day'), momentTwo);
  }

  isStartAfterEnd(leaveRequest: LeaveRequest) {
    return moment(leaveRequest.startDate).isAfter(leaveRequest.endDate, 'D');
  }
}
