import { inject, TestBed } from '@angular/core/testing';

import { EmergencyContactResolverService } from './emergency-contact-resolver.service';

describe('EmergencyContactResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [EmergencyContactResolverService]
    });
  });

  it('should be created', inject([EmergencyContactResolverService], (service: EmergencyContactResolverService) => {
    expect(service).toBeTruthy();
  }));
});
