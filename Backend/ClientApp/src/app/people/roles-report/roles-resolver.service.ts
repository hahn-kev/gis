import { Injectable } from '@angular/core';
import { PersonService } from '../person.service';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { RoleWithJob } from '../role';
import { Observable } from 'rxjs';
import { Year } from '../training-requirement/year';

@Injectable()
export class RolesResolverService implements Resolve<RoleWithJob[]> {
  resolve(route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<RoleWithJob[]> | Promise<RoleWithJob[]> | RoleWithJob[] {
    const year = new Year(Number(route.paramMap.get('year')) || Year.CurrentSchoolYear());
    if (route.data.supervisor) return this.personService.getSupervisorRoles(true, year.startOfYear, year.endOfYear);
    return this.personService.getRoles(true, year.startOfYear, year.endOfYear);
  }

  constructor(private personService: PersonService) {
  }

}
