import { Injectable } from '@angular/core';
import { MissionOrgService } from '../mission-org.service';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { MissionOrg } from '../mission-org';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class MissionOrgListResolverService implements Resolve<MissionOrg[]> {

  constructor(private missionOrgService: MissionOrgService) {
  }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<MissionOrg[]> | Promise<MissionOrg[]> | MissionOrg[] {
    return this.missionOrgService.list();
  }

}
