import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { MissionOrg } from '../mission-org';
import { Observable } from 'rxjs';
import { MissionOrgService } from '../mission-org.service';

@Injectable()
export class MissionOrgResolverService implements Resolve<MissionOrg>{

  constructor(private missionOrgService: MissionOrgService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<MissionOrg> | Promise<MissionOrg> | MissionOrg {
    if (route.params['id'] === 'new') return new MissionOrg();
    return this.missionOrgService.getById(route.params['id']);
  }

}
