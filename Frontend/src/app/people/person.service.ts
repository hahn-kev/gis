import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Person, PersonExtended, PersonWithDaysOfLeave, PersonWithOthers, StaffWithName } from './person';
import { Observable } from 'rxjs/Observable';
import { Role, RoleExtended } from './role';

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

  getPeopleWithDaysOfLeave(): Observable<PersonWithDaysOfLeave[]> {
    return this.http.get<PersonWithDaysOfLeave[]>('/api/person/leave');
  }

  updatePerson(person: PersonExtended): Promise<string> {
    return this.http.post('/api/person', person, {responseType: 'text'}).toPromise();
  }

  updateRole(role: Role): Promise<Role> {
    return this.http.post<Role>('/api/person/role', role).toPromise();
  }

  getRoles(canStartDuringRange: boolean, beginRange: Date, endRange: Date): Promise<RoleExtended[]> {
    const params = new HttpParams()
      .append('canStartDuringRange', canStartDuringRange.toString())
      .append('beginRange', beginRange.toISOString())
      .append('endRange', endRange.toISOString());
    return this.http.get<RoleExtended[]>('/api/person/role', {params: params}).toPromise();
  }

  getStaff(): Observable<StaffWithName[]> {
    return this.http.get<StaffWithName[]>('/api/person/staff');
  }
}
