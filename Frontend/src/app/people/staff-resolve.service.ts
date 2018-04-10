import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { PersonWithStaff} from './person';
import { PersonService } from './person.service';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class StaffResolveService implements Resolve<PersonWithStaff[]> {

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<PersonWithStaff[]> | Promise<PersonWithStaff[]> | PersonWithStaff[] {
    return this.personService.getStaffAll();
  }

  constructor(private personService: PersonService) {
  }

}
