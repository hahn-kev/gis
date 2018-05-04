import { Injectable } from '@angular/core';
import { PersonService } from '../../person.service';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { PersonWithStaffSummaries } from '../../person';
import { Observable } from 'rxjs';

@Injectable()
export class StaffSummariesResolveService implements Resolve<PersonWithStaffSummaries[]> {

  constructor(private personService: PersonService) {
  }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<PersonWithStaffSummaries[]> | Promise<PersonWithStaffSummaries[]> | PersonWithStaffSummaries[] {
    return this.personService.getStaffSummaries();
  }

}
