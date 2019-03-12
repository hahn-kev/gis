import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import {PersonWithRoleSummaries} from '../person';
import { Observable } from 'rxjs';
import { PersonService } from '../person.service';

@Injectable()
export class SchoolAidResolveService implements Resolve<PersonWithRoleSummaries[]> {

  constructor(private personService: PersonService) {
  }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<PersonWithRoleSummaries[]> | Promise<PersonWithRoleSummaries[]> | PersonWithRoleSummaries[] {
    return this.personService.getAllSchoolAidSummaries();
  }

}
