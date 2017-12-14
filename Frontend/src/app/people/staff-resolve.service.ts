import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { StaffWithName } from './person';
import { PersonService } from './person.service';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class StaffResolveService implements Resolve<StaffWithName[]> {

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<StaffWithName[]> | Promise<StaffWithName[]> | StaffWithName[] {
    return this.personService.getStaff();
  }

  constructor(private personService: PersonService) { }

}
