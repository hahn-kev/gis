import { TestBed, inject } from '@angular/core/testing';

import { StaffResolveService } from './staff-resolve.service';

describe('StaffResolveService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [StaffResolveService]
    });
  });

  it('should be created', inject([StaffResolveService], (service: StaffResolveService) => {
    expect(service).toBeTruthy();
  }));
});
