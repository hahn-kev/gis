import { inject, TestBed } from '@angular/core/testing';

import { EndorsementService } from './endorsement.service';

describe('EndorsementService',
  () => {
    beforeEach(() => {
      TestBed.configureTestingModule({
        providers: [EndorsementService]
      });
    });

    it('should be created',
      inject([EndorsementService],
        (service: EndorsementService) => {
          expect(service).toBeTruthy();
        }));
  });
