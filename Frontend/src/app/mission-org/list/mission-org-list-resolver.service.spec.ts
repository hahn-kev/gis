import { TestBed, inject } from '@angular/core/testing';

import { MissionOrgListResolverService } from './mission-org-list-resolver.service';

describe('MissionOrgListResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [MissionOrgListResolverService]
    });
  });

  it('should be created', inject([MissionOrgListResolverService], (service: MissionOrgListResolverService) => {
    expect(service).toBeTruthy();
  }));
});
