import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Education } from './education';

@Injectable({
  providedIn: 'root'
})
export class EducationService {

  constructor(private http: HttpClient) {
  }

  save(ed: Education) {
    return this.http.post<Education>('/api/education', ed);
  }

  deleteEducation(id: string) {
    return this.http.delete('/api/education/' + id, {responseType: 'text'});
  }
}
