import { inject, TestBed } from '@angular/core/testing';

import { StaffResolveService } from './staff-resolve.service';

xdescribe('StaffResolveService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [StaffResolveService]
    });
  });

  it('should be created', inject([StaffResolveService], (service: StaffResolveService) => {
    expect(service).toBeTruthy();
  }));
});
