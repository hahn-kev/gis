import { inject, TestBed } from '@angular/core/testing';

import { LeaveListResolverService } from './leave-list-resolver.service';

describe('LeaveListResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [LeaveListResolverService]
    });
  });

  it('should be created', inject([LeaveListResolverService], (service: LeaveListResolverService) => {
    expect(service).toBeTruthy();
  }));
});
