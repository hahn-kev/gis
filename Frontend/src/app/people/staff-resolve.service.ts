import { Injectable } from '@angular/core';
import { Resolve } from '@angular/router';
import { StaffWithName } from './person';
import { PersonService } from './person.service';

@Injectable()
export class StaffResolveService implements Resolve<StaffWithName[]> {

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<StaffWithName[]> | Promise<StaffWithName[]> | StaffWithName[] {
    return this.personService.getStaff();
  }

  constructor(private personService: PersonService) { }

}
