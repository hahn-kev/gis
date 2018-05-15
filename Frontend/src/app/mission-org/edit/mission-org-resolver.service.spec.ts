import { inject, TestBed } from '@angular/core/testing';

import { MissionOrgResolverService } from './mission-org-resolver.service';

describe('MissionOrgResolverService',
  () => {
    beforeEach(() => {
      TestBed.configureTestingModule({
        providers: [MissionOrgResolverService]
      });
    });

    it('should be created',
      inject([MissionOrgResolverService],
        (service: MissionOrgResolverService) => {
          expect(service).toBeTruthy();
        }));
  });
