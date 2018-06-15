import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Endorsement } from './endorsement';
import { Observable } from 'rxjs';
import { EndorsementService } from './endorsement.service';

@Injectable()
export class EndorsementResolverService implements Resolve<Endorsement> {

  constructor(private endorsementService: EndorsementService) {
  }

  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<Endorsement> | Promise<Endorsement> | Endorsement {
    if (route.params['id'] == 'new') return new Endorsement();
    return this.endorsementService.getEndorsementById(route.params['id']);
  }
}
