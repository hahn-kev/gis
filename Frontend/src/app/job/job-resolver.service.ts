import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { JobService } from './job.service';
import { Job } from './job';

@Injectable()
export class JobResolverService implements Resolve<Job> {

  constructor(private jobService: JobService) {
  }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<Job> | Promise<Job> | Job {
    if (route.params['id'] === 'new') return new Job();
    return this.jobService.get(route.params['id']);
  }

}
