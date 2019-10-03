import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { MissionOrgWithYearSummaries } from '../mission-org';
import { Observable } from 'rxjs';
import { MissionOrgService } from '../mission-org.service';

@Injectable()
export class MissionOrgResolverService implements Resolve<MissionOrgWithYearSummaries> {

  constructor(private missionOrgService: MissionOrgService) {
  }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<MissionOrgWithYearSummaries> |
    Promise<MissionOrgWithYearSummaries> |
    MissionOrgWithYearSummaries {
    if (route.params['id'] === 'new') return new MissionOrgWithYearSummaries();
    return this.missionOrgService.getById(route.params['id']);
  }

}
