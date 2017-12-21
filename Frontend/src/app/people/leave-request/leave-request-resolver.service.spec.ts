import { inject, TestBed } from '@angular/core/testing';

import { LeaveRequestResolverService } from './leave-request-resolver.service';

describe('LeaveRequestResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [LeaveRequestResolverService]
    });
  });

  it('should be created', inject([LeaveRequestResolverService], (service: LeaveRequestResolverService) => {
    expect(service).toBeTruthy();
  }));
});
