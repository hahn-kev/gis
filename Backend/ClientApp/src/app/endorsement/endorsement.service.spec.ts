import { inject, TestBed } from '@angular/core/testing';

import { EndorsementService } from './endorsement.service';

xdescribe('EndorsementService',
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
