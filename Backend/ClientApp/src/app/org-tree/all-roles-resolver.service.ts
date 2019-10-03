import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { RoleExtended } from '../people/role';
import { Observable } from 'rxjs';
import { JobService } from '../job/job.service';

@Injectable({
  providedIn: 'root'
})
export class AllRolesResolverService implements Resolve<RoleExtended[]> {

  constructor(private jobService: JobService) {
  }


  resolve(route: ActivatedRouteSnapshot,
          state: RouterStateSnapshot): Observable<RoleExtended[]> | Promise<RoleExtended[]> | RoleExtended[] {
    return this.jobService.getAllRoles();
  }
}
