import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Holiday } from '../people/leave-request/holiday';
import { Observable } from 'rxjs';
import { HolidayService } from './holiday.service';

@Injectable()
export class HolidayListResolverService implements Resolve<Holiday[]> {

  constructor(private holidayService: HolidayService) {
  }

  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<Holiday[]> | Promise<Holiday[]> | Holiday[] {
    return this.holidayService.listHolidays();
  }
}
