import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MissionOrg, MissionOrgWithNames } from './mission-org';
import { MissionOrgYearSummary } from './mission-org-year-summary';

@Injectable()
export class MissionOrgService {

  constructor(private http: HttpClient) {
  }

  list() {
    return this.http.get<MissionOrgWithNames[]>('/api/missionOrg/');
  }

  getById(id: string) {
    return this.http.get<MissionOrg>('/api/missionOrg/' + id);
  }

  delete(id: string) {
    return this.http.delete('/api/missionOrg/' + id).toPromise();
  }

  save(missionOrg: MissionOrg) {
    return this.http.post<MissionOrg>('/api/missionOrg', missionOrg).toPromise();
  }

  saveYear(year: MissionOrgYearSummary) {
    return this.http.post<MissionOrgYearSummary>('/api/missionOrg/year', year).toPromise();
  }

  deleteYear(id: string) {
    return this.http.delete('/api/missionOrg/year/' + id).toPromise();
  }
}
