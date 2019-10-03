import { inject, TestBed } from '@angular/core/testing';

import { StaffSummariesResolveService } from './staff-summaries-resolve.service';

xdescribe('StaffSummariesResolveService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [StaffSummariesResolveService]
    });
  });

  it('should be created', inject([StaffSummariesResolveService], (service: StaffSummariesResolveService) => {
    expect(service).toBeTruthy();
  }));
});
