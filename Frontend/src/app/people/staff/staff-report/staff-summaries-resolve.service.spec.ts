import { TestBed, inject } from '@angular/core/testing';

import { StaffSummariesResolveService } from './staff-summaries-resolve.service';

describe('StaffSummariesResolveService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [StaffSummariesResolveService]
    });
  });

  it('should be created', inject([StaffSummariesResolveService], (service: StaffSummariesResolveService) => {
    expect(service).toBeTruthy();
  }));
});
