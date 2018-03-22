import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { JobService } from './job.service';
import { JobWithRoles } from './job';

@Injectable()
export class JobResolverService implements Resolve<JobWithRoles> {

  constructor(private jobService: JobService) {
  }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<JobWithRoles> | Promise<JobWithRoles> | JobWithRoles {
    if (route.params['id'] === 'new') return new JobWithRoles();
    return this.jobService.get(route.params['id']);
  }

}
