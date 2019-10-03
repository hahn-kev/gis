import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Endorsement, RequiredEndorsement, StaffEndorsement } from './endorsement';

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

  saveRequiredEndorsement(requiredEndorsement: RequiredEndorsement) {
    return this.http.post<RequiredEndorsement>('/api/endorsement/required', requiredEndorsement).toPromise();
  }

  deleteRequiredEndorsement(id: string) {
    return this.http.delete('/api/endorsement/required/' + id, {responseType: 'text'}).toPromise();
  }

  saveStaffEndorsement(staffEndorsement: StaffEndorsement) {
    return this.http.post<StaffEndorsement>('/api/endorsement/staff', staffEndorsement).toPromise();
  }

  deleteStaffEndorsement(id: string) {
    return this.http.delete('/api/endorsement/staff/' + id, {responseType: 'text'}).toPromise();
  }
}
