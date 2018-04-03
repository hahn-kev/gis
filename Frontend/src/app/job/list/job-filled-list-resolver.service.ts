import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { JobWithFilledInfo } from 'app/job/job';
import { Observable } from 'rxjs/Observable';
import { JobService } from '../job.service';

@Injectable()
export class JobFilledListResolverService implements Resolve<JobWithFilledInfo[]> {

  constructor(private jobService: JobService) {
  }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<JobWithFilledInfo[]> | Promise<JobWithFilledInfo[]> | JobWithFilledInfo[] {
    return this.jobService.listWithFilledInfo();
  }
}
