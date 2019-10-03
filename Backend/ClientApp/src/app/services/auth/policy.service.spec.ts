import { inject, TestBed } from '@angular/core/testing';

import { PolicyService } from './policy.service';

xdescribe('PolicyService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PolicyService]
    });
  });

  it('should be created', inject([PolicyService], (service: PolicyService) => {
    expect(service).toBeTruthy();
  }));
});
