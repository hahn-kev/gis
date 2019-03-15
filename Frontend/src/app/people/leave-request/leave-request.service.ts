import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LeaveRequest, LeaveRequestPublic, LeaveRequestWithNames } from './leave-request';
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

  listPublic(): Observable<LeaveRequestPublic[]> {
    return this.http.get<LeaveRequestPublic[]>('/api/leaveRequest/public');
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

  weekDays(leaveRequest: LeaveRequest, holidays: Holiday[]): { days: number, countedHolidays: string } {
    let days = this.weekDaysBetween(leaveRequest.startDate, leaveRequest.endDate);
    const countedHolidays = [];
    for (const holiday of holidays) {
      let overlapping = this.overlapingHolidayDays(leaveRequest, holiday);
      if (overlapping > 0) countedHolidays.push(holiday.name);
      days -= overlapping;
    }

    return {days, countedHolidays: countedHolidays.join(', ')};
  }

  overlapingHolidayDays(leaveRequest: LeaveRequest, holiday: Holiday) {
    const leaveStart = moment(leaveRequest.startDate);
    const leaveEnd = moment(leaveRequest.endDate);
    const holidayStart = moment(holiday.start);
    const holidayEnd = moment(holiday.end);
    if (holidayEnd.isBefore(leaveStart) || holidayStart.isAfter(leaveEnd)) {
      return 0;
    }
    if (holidayStart.isSame(holidayEnd, 'day')) {
      return holidayStart.isBetween(leaveStart, leaveEnd, 'day', '[]') ? 1 : 0;
    }
    if (leaveStart.isSame(leaveEnd, 'day')) {
      return leaveStart.isBetween(holidayStart, holidayEnd, 'day', '[]') ? 1 : 0;
    }
    //key leave = () holiday = []
    //([])
    if (leaveStart.isSameOrBefore(holidayStart) && holidayEnd.isSameOrBefore(leaveEnd)) {
      return this.weekDaysBetween(holidayStart, holidayEnd);
    }
    //[()]
    if (holidayStart.isSameOrBefore(leaveStart) && leaveEnd.isSameOrBefore(holidayEnd)) {
      return this.weekDaysBetween(leaveStart, leaveEnd);
    }
    //([)]
    if (holidayEnd.isAfter(leaveEnd) && holidayStart.isAfter(leaveStart) && holidayStart.isSameOrBefore(leaveEnd)) {
      return this.weekDaysBetween(holidayStart, leaveEnd);
    }
    //[(])
    if (holidayStart.isBefore(leaveStart) && holidayEnd.isBefore(leaveEnd) && holidayEnd.isSameOrAfter(leaveStart)) {
      return this.weekDaysBetween(leaveStart, holidayEnd);
    }
    console.error('Not sure how holiday: ' + holiday.start + ' -> ' + holiday.end + ' overlaps with ' + leaveRequest.startDate + ' -> ' + leaveRequest.endDate);
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
