import { Injectable } from '@angular/core';
import { Person } from '../person';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { PersonService } from '../person.service';

@Injectable()
export class PeopleResolveService implements Resolve<Person[]> {
  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<Person[]> | Promise<Person[]> | Person[] {
    return this.personService.getAll();
  }

  constructor(private personService: PersonService) {
  }

}
