import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MissionOrg } from './mission-org';

@Injectable()
export class MissionOrgService {

  constructor(private http: HttpClient) {
  }

  list() {
    return this.http.get<MissionOrg[]>('/api/missionOrg/');
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
}
