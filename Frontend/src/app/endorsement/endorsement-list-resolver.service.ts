import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Endorsement } from './endorsement';
import { Observable } from 'rxjs';
import { EndorsementService } from './endorsement.service';

@Injectable()
export class EndorsementListResolverService implements Resolve<Endorsement[]> {

  constructor(private endorsementService: EndorsementService) {
  }

  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<Endorsement[]> | Promise<Endorsement[]> | Endorsement[] {
    return this.endorsementService.list();
  }
}
