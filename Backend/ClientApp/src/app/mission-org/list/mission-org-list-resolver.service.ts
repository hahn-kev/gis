import { Injectable } from '@angular/core';
import { MissionOrgService } from '../mission-org.service';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { MissionOrgWithNames } from '../mission-org';
import { Observable } from 'rxjs';

@Injectable()
export class MissionOrgListResolverService implements Resolve<MissionOrgWithNames[]> {

  constructor(private missionOrgService: MissionOrgService) {
  }

  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<MissionOrgWithNames[]> | Promise<MissionOrgWithNames[]> | MissionOrgWithNames[] {
    return this.missionOrgService.list();
  }

}
