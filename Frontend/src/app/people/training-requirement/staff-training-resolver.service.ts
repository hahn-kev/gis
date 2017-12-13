import { Injectable } from '@angular/core';
import { TrainingRequirementService } from './training-requirement.service';
import { Resolve } from '@angular/router';
import { StaffTraining } from './staff-training';

@Injectable()
export class StaffTrainingResolverService implements Resolve<StaffTraining[]> {


  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<StaffTraining[]> | Promise<StaffTraining[]> | StaffTraining[] {
    return this.trainingService.getStaffTrainingByYear(route.params['year']);
  }

  constructor(private trainingService: TrainingRequirementService) {
  }

}
