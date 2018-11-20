import { inject, TestBed } from '@angular/core/testing';

import { MissionOrgService } from './mission-org.service';

xdescribe('MissionOrgService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [MissionOrgService]
    });
  });

  it('should be created', inject([MissionOrgService], (service: MissionOrgService) => {
    expect(service).toBeTruthy();
  }));
});
