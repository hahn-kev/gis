import { Injectable } from '@angular/core';
import { Holiday } from '../people/leave-request/holiday';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class HolidayService {

  constructor(private http: HttpClient) {
  }

  listHolidays() {
    return this.http.get<Holiday[]>('/api/holiday');
  }

  currentHolidays() {
    return this.http.get<Holiday[]>('/api/holiday/current');
  }

  getHolidayById(id: string) {
    return this.http.get<Holiday>('/api/holiday/' + id);
  }

  saveHoliday(holiday: Holiday) {
    return this.http.post<Holiday>('/api/holiday', holiday);
  }

  deleteHoliday(holidayId: string) {
    return this.http.delete('/api/holiday/' + holidayId);
  }
}
