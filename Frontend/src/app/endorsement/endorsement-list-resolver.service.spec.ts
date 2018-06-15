import { inject, TestBed } from '@angular/core/testing';

import { EndorsementListResolverService } from './endorsement-list-resolver.service';

describe('EndorsementListResolverService',
  () => {
    beforeEach(() => {
      TestBed.configureTestingModule({
        providers: [EndorsementListResolverService]
      });
    });

    it('should be created',
      inject([EndorsementListResolverService],
        (service: EndorsementListResolverService) => {
          expect(service).toBeTruthy();
        }));
  });
