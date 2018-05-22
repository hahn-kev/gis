import { Injectable } from '@angular/core';
import { JobService } from './job.service';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Job } from 'app/job/job';
import { Observable } from 'rxjs';

@Injectable()
export class JobListResolverService implements Resolve<Job[]> {

  constructor(private jobService: JobService) {
  }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<Job[]> | Promise<Job[]> | Job[] {
    return this.jobService.simpleList();
  }
}
