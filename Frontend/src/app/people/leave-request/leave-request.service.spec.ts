import { TestBed, inject } from '@angular/core/testing';

import { LeaveRequestService } from './leave-request.service';

describe('LeaveRequestService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [LeaveRequestService]
    });
  });

  it('should be created', inject([LeaveRequestService], (service: LeaveRequestService) => {
    expect(service).toBeTruthy();
  }));
});
