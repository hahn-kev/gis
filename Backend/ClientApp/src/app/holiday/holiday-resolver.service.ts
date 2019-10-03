import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Holiday } from '../people/leave-request/holiday';
import { Observable } from 'rxjs';
import { HolidayService } from './holiday.service';

@Injectable({
  providedIn: 'root'
})
export class HolidayResolverService implements Resolve<Holiday> {

  constructor(private holidayService: HolidayService) {
  }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<Holiday> | Promise<Holiday> | Holiday {
    if (route.params['id'] == 'new') return new Holiday();
    return this.holidayService.getHolidayById(route.params['id']);
  }
}
