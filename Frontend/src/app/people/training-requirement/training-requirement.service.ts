import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TrainingRequirement } from './training-requirement';
import { Observable } from 'rxjs/Observable';
import { combineLatest, map } from 'rxjs/operators';
import { Year } from './year';
import { StaffTraining } from './staff-training';
import { RequirementWithStaff, StaffWithTraining } from './training-report/requirement-with-staff';
import { StaffWithName } from '../person';
import 'rxjs/add/operator/do';

@Injectable()
export class TrainingRequirementService {

  constructor(private http: HttpClient) {
  }

  list(): Observable<TrainingRequirement[]> {
    return this.http.get<TrainingRequirement[]>('/api/training');
  }

  getStaffTrainingByYear(year: number) {
    return this.http.get<StaffTraining[]>('/api/training/staff/' + year);
  }

  getStaffTrainingByYearMapped(year: number): Observable<Map<string, StaffTraining>> {
    return this.getStaffTrainingByYear(year).map(staffTrainings => {
      return new Map<string, StaffTraining>(staffTrainings
        .map((training): [string, StaffTraining] => [StaffTraining.getKey(training), training])
      );
    });
  }

  get(id: string): Observable<Object> {
    return this.http.get<TrainingRequirement>('/api/training/' + id);
  }

  save(training: TrainingRequirement): Promise<Object> {
    return this.http.post<TrainingRequirement>('/api/training/', training).toPromise();
  }

  saveStaffTraining(staffTraining: StaffTraining) {
    return this.http.post<StaffTraining>('/api/training/staff', staffTraining).toPromise();
  }

  markAllComplete(staffList: string[], requirementId: string, completeDate: Date) {
    return this.http.post('/api/training/staff/allComplete', staffList,
      {
        params: {
          'completeDate': completeDate.toISOString(),
          'requirementId': requirementId
        }, responseType: 'text'
      }).toPromise();
  }

  years(): Year[] {
    let today = new Date();
    let years = new Array<Year>(today.getUTCFullYear() - 2000 + 3);
    for (let i = 0; i < years.length; i++) {
      let display;
      if (i < 9) {
        display = `0${i} - 0${i + 1}`;
      } else if (i == 9) {
        display = '09 - 10';
      } else {
        display = `${i} - ${i + 1}`;
      }
      years[i] = new Year(i + 2000, display);
    }
    return years.reverse();
  }

  buildRequirementsWithStaff(staff: Observable<StaffWithName[]>,
                             requirements: Observable<TrainingRequirement[]>,
                             staffTraining: Observable<Map<string, StaffTraining>>,
                             year: Observable<number>): Observable<RequirementWithStaff[]> {
    return staffTraining.pipe(
      combineLatest(staff, requirements, year),
      map(([staffTraining, staff, requirements, year]) => {
        return requirements
          .filter(requirement => requirement.firstYear <= year && (!requirement.lastYear || requirement.lastYear >= year))
          .map(this.buildRequirementWithStaff.bind(this, staff, staffTraining));
      }));
  }

  buildRequirementWithStaff(staff: StaffWithName[],
                            staffTraining: Map<string, StaffTraining>,
                            requirement: TrainingRequirement) {
    return new RequirementWithStaff(requirement,
      staff.map(staff => new StaffWithTraining(staff, staffTraining.get(staff.id + '_' + requirement.id)))
    );
  }

}
