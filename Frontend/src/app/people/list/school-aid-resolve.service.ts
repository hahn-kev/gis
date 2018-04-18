import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Person } from '../person';
import { Observable } from 'rxjs/Observable';
import { PersonService } from '../person.service';

@Injectable()
export class SchoolAidResolveService implements Resolve<Person[]> {

  constructor(private personService: PersonService) {
  }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<Person[]> | Promise<Person[]> | Person[] {
    return this.personService.getAllSchoolAids();
  }

}
