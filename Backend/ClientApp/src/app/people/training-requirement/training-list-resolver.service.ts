import { Injectable } from '@angular/core';
import { TrainingRequirementService } from './training-requirement.service';
import { TrainingRequirement } from './training-requirement';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

@Injectable()
export class TrainingListResolverService implements Resolve<TrainingRequirement[]> {
  resolve(route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<TrainingRequirement[]> | Promise<TrainingRequirement[]> | TrainingRequirement[] {
    return this.trainingService.list();
  }

  constructor(private trainingService: TrainingRequirementService) {
  }

}
