import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MissionOrg, MissionOrgWithNames, MissionOrgWithYearSummaries } from './mission-org';
import { MissionOrgYearSummary } from './mission-org-year-summary';
import { Person } from '../people/person';

@Injectable()
export class MissionOrgService {

  constructor(private http: HttpClient) {
  }

  list() {
    return this.http.get<MissionOrgWithNames[]>('/api/missionOrg/');
  }

  getById(id: string) {
    return this.http.get<MissionOrgWithYearSummaries>('/api/missionOrg/' + id);
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

  listPeople(orgId: string) {
    return this.http.get<Person[]>(`/api/missionOrg/${orgId}/people`);
  }
}
