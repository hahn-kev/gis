import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Person, PersonExtended } from './person';
import { Observable } from 'rxjs/Observable';
import { Role } from './role';

@Injectable()
export class PersonService {

  constructor(private http: HttpClient) {
  }

  getPerson(id: string): Observable<PersonExtended> {
    return this.http.get<PersonExtended>('/api/person/' + id);
  }

  getAll(): Observable<Person[]> {
    return this.http.get<Person[]>('/api/person');
  }

  updatePerson(person: PersonExtended): Promise<string> {
    return this.http.post('/api/person', person, {responseType: 'text'}).toPromise();
  }

  updateRole(role: Role): Promise<Role> {
    return this.http.post<Role>('/api/person/role', role).toPromise();
  }
}
