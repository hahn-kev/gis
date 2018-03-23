import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Grade } from './grade';

@Injectable()
export class GradeService {

  constructor(private http: HttpClient) {
  }

  list() {
    return this.http.get<Grade[]>('/api/job/grade');
  }

  get(id: string) {
    return this.http.get<Grade>('/api/job/grade/' + id);
  }

  save(Grade: Grade) {
    return this.http.post<Grade>('/api/job/grade', Grade).toPromise();
  }

  delete(gradeId: string) {
    return this.http.delete('/api/job/grade/' + gradeId, {responseType: 'text'});
  }
}
