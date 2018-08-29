import {inject, TestBed} from '@angular/core/testing';

import {LeaveRequestSupervisorResolverService} from './leave-request-supervisor-resolver.service';

describe('LeaveRequestSupervisorResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [LeaveRequestSupervisorResolverService]
    });
  });

  it('should be created', inject([LeaveRequestSupervisorResolverService], (service: LeaveRequestSupervisorResolverService) => {
    expect(service).toBeTruthy();
  }));
});
