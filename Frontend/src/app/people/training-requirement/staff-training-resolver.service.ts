import { Injectable } from '@angular/core';
import { TrainingRequirementService } from './training-requirement.service';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { StaffTraining } from './staff-training';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class StaffTrainingResolverService implements Resolve<StaffTraining[]> {


  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<StaffTraining[]> | Promise<StaffTraining[]> | StaffTraining[] {
    return this.trainingService.getStaffTrainingByYear(route.params['year'] || new Date().getUTCFullYear());
  }

  constructor(private trainingService: TrainingRequirementService) {
  }

}
