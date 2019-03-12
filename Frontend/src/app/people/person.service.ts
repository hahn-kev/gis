import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import {
  Person,
  PersonWithDaysOfLeave,
  PersonWithOthers,
  PersonWithRoleSummaries,
  PersonWithStaff,
  PersonWithStaffSummaries
} from './person';
import { Observable } from 'rxjs';
import { Role, RoleWithJob } from './role';
import { EmergencyContactExtended } from './emergency-contact';
import { StaffWithName, StaffWithRoles } from './staff';

@Injectable()
export class PersonService {

  constructor(private http: HttpClient) {
  }

  getPerson(id: string): Observable<PersonWithOthers> {
    return this.http.get<PersonWithOthers>('/api/person/' + id);
  }

  getAll(): Observable<Person[]> {
    return this.http.get<Person[]>('/api/person');
  }

  getAllSchoolAids(): Observable<Person[]> {
    return this.http.get<Person[]>('/api/person/school-aids');

  }
  getAllSchoolAidSummaries(): Observable<PersonWithRoleSummaries[]> {
    return this.http.get<PersonWithRoleSummaries[]>('/api/person/school-aids/summaries');

  }

  getPeopleWithDaysOfLeave(): Observable<PersonWithDaysOfLeave[]> {
    return this.http.get<PersonWithDaysOfLeave[]>('/api/person/leave');
  }

  updatePerson(person: PersonWithStaff, isSelf = false): Promise<PersonWithStaff> {
    if (isSelf) return this.http.post<PersonWithStaff>('/api/person/self', person).toPromise();
    return this.http.post<PersonWithStaff>('/api/person', person).toPromise();
  }

  deletePerson(personId: string): Promise<string> {
    return this.http.delete('/api/person/' + personId, {responseType: 'text'}).toPromise();
  }

  updateRole(role: Role): Promise<Role> {
    return this.http.post<Role>('/api/person/role', role).toPromise();
  }

  deleteRole(roleId: string): Promise<string> {
    return this.http.delete('/api/person/role/' + roleId, {responseType: 'text'}).toPromise();
  }

  getRoles(canStartDuringRange: boolean, beginRange: Date, endRange: Date): Promise<RoleWithJob[]> {
    const params = new HttpParams()
      .append('canStartDuringRange', canStartDuringRange.toString())
      .append('beginRange', beginRange.toISOString())
      .append('endRange', endRange.toISOString());
    return this.http.get<RoleWithJob[]>('/api/person/role', {params: params}).toPromise();
  }

  getStaff(): Observable<StaffWithName[]> {
    return this.http.get<StaffWithName[]>('/api/person/staff');
  }

  getStaffAll() {
    return this.http.get<PersonWithStaff[]>('/api/person/staff/all');
  }

  getStaffSummaries() {
    return this.http.get<PersonWithStaffSummaries[]>('/api/person/staff/summaries');
  }

  getStaffWithRoles() {
    return this.http.get<StaffWithRoles[]>('/api/person/staff/roles');
  }

  getEmergencyContacts(personId: string) {
    return this.http.get<EmergencyContactExtended[]>(`/api/person/${personId}/emergency`).toPromise();
  }

  updateEmergencyContact(emergencyContact: EmergencyContactExtended) {
    return this.http.post<EmergencyContactExtended>('/api/person/emergency', emergencyContact).toPromise();
  }

  deleteEmergencyContact(id: string) {
    return this.http.delete('/api/person/emergency/' + id, {responseType: 'text'}).toPromise();
  }
}
