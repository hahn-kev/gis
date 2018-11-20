import { inject, TestBed } from '@angular/core/testing';

import { EndorsementResolverService } from './endorsement-resolver.service';

xdescribe('EndorsementResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [EndorsementResolverService]
    });
  });

  it('should be created', inject([EndorsementResolverService], (service: EndorsementResolverService) => {
    expect(service).toBeTruthy();
  }));
});
