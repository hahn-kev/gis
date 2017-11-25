import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { PersonExtended } from './person';
import { Observable } from 'rxjs/Observable';
import { PersonService } from './person.service';

@Injectable()
export class PersonResolverService implements Resolve<PersonExtended> {
  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<PersonExtended> | Promise<PersonExtended> | PersonExtended {
    if (route.params['id'] === 'new') {
      return new PersonExtended();
    }
    return this.personService.getPerson(route.params['id']);
  }

  constructor(private personService: PersonService) {
  }
}
