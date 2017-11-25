import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Person, PersonExtended } from './person';
import { Observable } from 'rxjs/Observable';

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

  update(person: PersonExtended) {
    return this.http.post('/api/person', person, {responseType: 'text'}).toPromise();
  }
}
