import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Endorsement } from './endorsement';

@Injectable({
  providedIn: 'root'
})
export class EndorsementService {

  constructor(private http: HttpClient) {
  }

  list() {
    return this.http.get<Endorsement[]>('/api/endorsement');
  }

  getEndorsementById(id: string) {
    return this.http.get<Endorsement>('/api/endorsement/' + id);
  }

  saveEndorsement(endorsement: Endorsement) {
    return this.http.post<Endorsement>('/api/endorsement', endorsement);
  }

  deleteEndorsement(id: string) {
    return this.http.delete('/api/endorsement/' + id, {responseType: 'text'});
  }
}
