import { Injectable } from '@angular/core';
import { TrainingRequirementService } from './training-requirement.service';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { StaffTraining } from './staff-training';
import { Observable } from 'rxjs';
import { Year } from './year';

@Injectable()
export class StaffTrainingResolverService implements Resolve<Map<string, StaffTraining>> {


  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<Map<string, StaffTraining>> | Promise<Map<string, StaffTraining>> | Map<string, StaffTraining> {
    return this.trainingService.getStaffTrainingByYearMapped(route.params['year'] ||  Year.CurrentSchoolYear());
  }

  constructor(private trainingService: TrainingRequirementService) {
  }

}
