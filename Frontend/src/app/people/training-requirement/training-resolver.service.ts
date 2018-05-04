import { Injectable } from '@angular/core';
import { TrainingRequirementService } from './training-requirement.service';
import { TrainingRequirement } from './training-requirement';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

@Injectable()
export class TrainingResolverService implements Resolve<TrainingRequirement> {
  resolve(route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<TrainingRequirement> | Promise<TrainingRequirement> | TrainingRequirement {
    if (route.params['id'] == 'new') return new TrainingRequirement();
    return this.trainingService.get(route.params['id']);
  }

  constructor(private trainingService: TrainingRequirementService) {
  }

}
