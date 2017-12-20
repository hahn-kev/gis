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
import 'rxjs/add/operator/map';
import { OrgGroup } from 'app/people/groups/org-group';
import { GroupService } from 'app/people/groups/group.service';

@Injectable()
export class TrainingRequirementService {

  constructor(private http: HttpClient, private groupService: GroupService) {
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
      combineLatest(staff, requirements, year, Observable.of([])),
      map(([staffTraining, staff, requirements, year, orgGroups]) => {
        return requirements
          .filter(this.isInYear.bind(this, year))
          .map(this.buildRequirementWithStaff.bind(this, staff, staffTraining, orgGroups));
      }));
  }


  isInYear(year: number, requirement: TrainingRequirement): boolean {
    return requirement.firstYear <= year && (!requirement.lastYear || requirement.lastYear >= year);
  }

  buildRequirementWithStaff(staff: StaffWithName[],
                            staffTraining: Map<string, StaffTraining>,
                            orgGroups: OrgGroup[],
                            requirement: TrainingRequirement): RequirementWithStaff {
    let training = staff.filter(this.isInOrgGroup.bind(this, orgGroups, requirement))
      .map(staff => new StaffWithTraining(staff, staffTraining.get(staff.id + '_' + requirement.id)));
    return new RequirementWithStaff(requirement, training);
  }

  isInOrgGroup(orgGroups: OrgGroup[] | Map<string, OrgGroup>, requirement: TrainingRequirement, staff: StaffWithName) {
    if (requirement.scope != 'Department') return true;
    if (requirement.departmentId == null) throw new Error('training requirement corupt, missing department id');
    return this.groupService.isChildOf(staff.orgGroupId, requirement.departmentId, orgGroups);
  }
}
