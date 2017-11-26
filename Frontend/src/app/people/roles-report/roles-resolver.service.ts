import { Injectable } from '@angular/core';
import { PersonService } from '../person.service';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { RoleExtended } from '../role';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class RolesResolverService implements Resolve<RoleExtended[]> {
  resolve(route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<RoleExtended[]> | Promise<RoleExtended[]> | RoleExtended[] {
    const oneYearInMs = 1000 * 60 * 60 * 24 * 365;
    return this.personService.getRoles(route.params['start'] == 'during',

      new Date(route.queryParams['begin'] || Date.now() - oneYearInMs / 2),
      new Date(route.queryParams['end'] || Date.now() + oneYearInMs / 2));
  }

  constructor(private personService: PersonService) {
  }

}
