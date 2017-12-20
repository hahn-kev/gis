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
import 'rxjs/add/observable/of';

@Injectable()
export class TrainingRequirementService {

  constructor(private http: HttpClient, private groupService: GroupService) {
  }

  list(): Observable<TrainingRequirement[]> {
    return this.http.get<TrainingRequirement[]>('/api/training');
  }

  getStaffTrainingByYear(year: number): Observable<StaffTraining[]> {
    return this.http.get<StaffTraining[]>('/api/training/staff/' + year);
  }

  getStaffTrainingByYearMapped(year: number): Observable<Map<string, StaffTraining>> {
    return this.getStaffTrainingByYear(year).map(staffTrainings => {
      return new Map<string, StaffTraining>(staffTrainings
        .map((training): [string, StaffTraining] => [StaffTraining.getKey(training), training])
      );
    });
  }

  get(id: string): Observable<TrainingRequirement> {
    return this.http.get<TrainingRequirement>('/api/training/' + id);
  }

  save(training: TrainingRequirement): Promise<TrainingRequirement> {
    return this.http.post<TrainingRequirement>('/api/training/', training).toPromise();
  }

  saveStaffTraining(staffTraining: StaffTraining): Promise<StaffTraining> {
    return this.http.post<StaffTraining>('/api/training/staff', staffTraining).toPromise();
  }

  markAllComplete(staffList: string[], requirementId: string, completeDate: Date): Promise<string> {
    return this.http.post('/api/training/staff/allComplete', staffList,
      {
        params: {
          'completeDate': completeDate.toISOString(),
          'requirementId': requirementId
        }, responseType: 'text'
      }).toPromise();
  }

  years(): Year[] {
    const today = new Date();
    const years = new Array<Year>(today.getUTCFullYear() - 2000 + 3);
    for (let i = 0; i < years.length; i++) {
      let display;
      if (i < 9) {
        display = `0${i} - 0${i + 1}`;
      } else if (i === 9) {
        display = '09 - 10';
      } else {
        display = `${i} - ${i + 1}`;
      }
      years[i] = new Year(i + 2000, display);
    }
    return years.reverse();
  }

  buildRequirementsWithStaff(staffObservable: Observable<StaffWithName[]>,
                             requirementsObservable: Observable<TrainingRequirement[]>,
                             staffTrainingObservable: Observable<Map<string, StaffTraining>>,
                             yearObservable: Observable<number>): Observable<RequirementWithStaff[]> {
    return staffTrainingObservable.pipe(
      combineLatest(staffObservable, requirementsObservable, yearObservable, Observable.of([])),
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
    const training = staff.filter(this.isInOrgGroup.bind(this, orgGroups, requirement))
      .map(staffMember => new StaffWithTraining(staffMember, staffTraining.get(staffMember.id + '_' + requirement.id)));
    return new RequirementWithStaff(requirement, training);
  }

  isInOrgGroup(orgGroups: OrgGroup[] | Map<string, OrgGroup>,
               requirement: TrainingRequirement,
               staff: StaffWithName): boolean {
    if (requirement.scope !== 'Department') {
      return true;
    }
    if (requirement.departmentId == null) {
      throw new Error('training requirement corupt, missing department id');
    }
    return this.groupService.isChildOf(staff.orgGroupId, requirement.departmentId, orgGroups);
  }
}
