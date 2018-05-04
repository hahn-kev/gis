import { Injectable } from '@angular/core';
import { PersonService } from './person.service';
import { EmergencyContactExtended } from './emergency-contact';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

@Injectable()
export class EmergencyContactResolverService implements Resolve<EmergencyContactExtended[]> {
  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<EmergencyContactExtended[]> | Promise<EmergencyContactExtended[]> | EmergencyContactExtended[] {
    if (route.params['id'] === 'new') return [];
    return this.personService.getEmergencyContacts(route.params['id']);
  }

  constructor(private personService: PersonService) {
  }

}
