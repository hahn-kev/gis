import { inject, TestBed } from '@angular/core/testing';

import { PolicyServiceService } from './policy-service.service';

describe('PolicyServiceService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PolicyServiceService]
    });
  });

  it('should be created', inject([PolicyServiceService], (service: PolicyServiceService) => {
    expect(service).toBeTruthy();
  }));
});
