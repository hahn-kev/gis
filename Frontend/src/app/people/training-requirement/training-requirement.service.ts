import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TrainingRequirement } from './training-requirement';
import { combineLatest, Observable } from 'rxjs';
import { debounceTime, map } from 'rxjs/operators';
import { StaffTraining, StaffTrainingWithRequirement } from './staff-training';
import { RequirementWithStaff, StaffWithTraining } from './training-report/requirement-with-staff';
import { StaffWithRoles } from '../staff';
import { OrgGroup, OrgGroupWithSupervisor } from 'app/people/groups/org-group';
import { GroupService } from 'app/people/groups/group.service';
import { Year } from './year';

@Injectable()
export class TrainingRequirementService {

  constructor(private http: HttpClient, private groupService: GroupService) {
  }

  list(): Observable<TrainingRequirement[]> {
    return this.http.get<TrainingRequirement[]>('/api/training');
  }

  listMapped(): Observable<Map<string, TrainingRequirement>> {
    return this.list().pipe(map(value =>
      new Map<string, TrainingRequirement>(value
        .map((training): [string, TrainingRequirement] => [training.id, training]))
    ));
  }

  getTrainingByStaffId(staffId: string) {
    return this.http.get<StaffTrainingWithRequirement[]>('/api/training/staff/' + staffId);
  }

  getStaffTrainingByYear(year: number): Observable<StaffTraining[]> {
    return this.http.get<StaffTraining[]>('/api/training/staff/year/' + year);
  }

  getStaffTrainingByYearMapped(year: number): Observable<Map<string, StaffTraining>> {
    return this.getStaffTrainingByYear(year).pipe(map(staffTrainings => {
      return new Map<string, StaffTraining>(staffTrainings
        .map((training): [string, StaffTraining] => [StaffTraining.getKey(training), training])
      );
    }));
  }

  get(id: string): Observable<TrainingRequirement> {
    return this.http.get<TrainingRequirement>('/api/training/' + id);
  }

  save(training: TrainingRequirement): Promise<TrainingRequirement> {
    return this.http.post<TrainingRequirement>('/api/training/', training).toPromise();
  }

  deleteRequirement(id: string): Promise<string> {
    return this.http.delete('/api/training/' + id, {responseType: 'text'}).toPromise();
  }

  saveStaffTraining(staffTraining: StaffTraining): Promise<StaffTraining> {
    return this.http.post<StaffTraining>('/api/training/staff', staffTraining).toPromise();
  }

  deleteStaffTraining(id: string): Promise<string> {
    return this.http.delete('/api/training/staff/' + id, {responseType: 'text'}).toPromise();
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

  buildRequirementsWithStaff(staffObservable: Observable<StaffWithRoles[]>,
                             orgGroupsObservable: Observable<OrgGroupWithSupervisor[]>,
                             requirementsObservable: Observable<TrainingRequirement[]>,
                             staffTrainingObservable: Observable<Map<string, StaffTraining>>,
                             yearObservable: Observable<number>,
                             showCompletedObservable: Observable<boolean>): Observable<RequirementWithStaff[]> {
    return combineLatest(staffTrainingObservable,
      staffObservable,
      requirementsObservable,
      yearObservable,
      showCompletedObservable,
      orgGroupsObservable)
      .pipe(
        debounceTime(20),
        map(([staffTraining, staff, requirements, year, showCompleted, orgGroups]) => {
          return requirements
            .filter(this.isInYear.bind(this, year))
            // .map(this.buildRequirementWithStaff.bind(this, staff, staffTraining, orgGroups, showCompleted))
            .map(
              requirement => this.buildRequirementWithStaff(staff,
                staffTraining,
                orgGroups,
                showCompleted,
                requirement,
                year))
            .filter((requirement, i, a) => requirement.totalStaff > 0 && showCompleted ?
              true :
              (requirement.staffsWithTraining.length > 0));
        }));
  }


  isInYear(year: number, requirement: TrainingRequirement): boolean {
    return requirement.firstYear <= year && (!requirement.lastYear || requirement.lastYear >= year);
  }

  buildRequirementWithStaff(staff: StaffWithRoles[],
                            staffTraining: Map<string, StaffTraining>,
                            orgGroups: OrgGroup[],
                            showCompleted: boolean,
                            requirement: TrainingRequirement,
                            year: number): RequirementWithStaff {
    const trainingForStaff = staff
      .map(staffMember => new StaffWithTraining(staffMember,
        staffTraining.get(staffMember.staffWithName.id + '_' + requirement.id)))
      .filter(this.matchesTrainingSpecs.bind(this, orgGroups, requirement, year));
    const trainingFilteredByComplete = trainingForStaff.filter(this.filterCompletedTraining.bind(this, showCompleted));
    return new RequirementWithStaff(requirement,
      trainingFilteredByComplete,
      orgGroups.find(value => value.id == requirement.departmentId),
      trainingForStaff.length);
  }

  filterCompletedTraining(showCompleted: boolean, staffTraining: StaffWithTraining) {
    if (showCompleted) return true;
    return staffTraining.training.completedDate == null;
  }

  matchesTrainingSpecs(orgGroups: OrgGroup[] | Map<string, OrgGroup>,
                       requirement: TrainingRequirement,
                       year: number,
                       staff: StaffWithTraining) {
    if (requirement.jobScope && requirement.jobScope.length > 0) {
      if (!requirement.jobScope.some(value => staff.roles.some(role => role.job.type == value))) return false;
      if (!staff.roles.some(role => Year.dateRangeIntersectsWithYear(role.startDate, role.endDate, year))) return false;
    }
    return this.isInOrgGroup(orgGroups, requirement, staff);
  }

  isInOrgGroup(orgGroups: OrgGroup[] | Map<string, OrgGroup>,
               requirement: TrainingRequirement,
               staff: StaffWithTraining): boolean {
    if (requirement.scope !== 'Department') {
      return true;
    }
    if (requirement.departmentId == null) {
      throw new Error('training requirement corupt, missing department id');
    }
    if (staff.roles.length > 0) {
      for (let role of staff.roles) {
        if (this.groupService.isChildOf(role.job.orgGroupId, requirement.departmentId, orgGroups)) return true;
      }
    }
    return this.groupService.isChildOf(staff.staff.orgGroupId, requirement.departmentId, orgGroups);
  }
}
