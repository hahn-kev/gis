import { inject, TestBed } from '@angular/core/testing';

import { MissionOrgListResolverService } from './mission-org-list-resolver.service';

xdescribe('MissionOrgListResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [MissionOrgListResolverService]
    });
  });

  it('should be created', inject([MissionOrgListResolverService], (service: MissionOrgListResolverService) => {
    expect(service).toBeTruthy();
  }));
});
