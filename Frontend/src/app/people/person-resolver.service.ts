import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { PersonWithOthers } from './person';
import { Observable } from 'rxjs/Observable';
import { PersonService } from './person.service';

@Injectable()
export class PersonResolverService implements Resolve<PersonWithOthers> {
  resolve(route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<PersonWithOthers> | Promise<PersonWithOthers> | PersonWithOthers {
    if (route.params['id'] === 'new') {
      return new PersonWithOthers();
    }
    return this.personService.getPerson(route.params['id']);
  }

  constructor(private personService: PersonService) {
  }
}
