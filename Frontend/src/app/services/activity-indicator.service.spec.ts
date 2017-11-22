import { TestBed, inject } from '@angular/core/testing';

import { ActivityIndicatorService } from './activity-indicator.service';

describe('ActivityIndicatorService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ActivityIndicatorService]
    });
  });

  it('should be created', inject([ActivityIndicatorService], (service: ActivityIndicatorService) => {
    expect(service).toBeTruthy();
  }));
});
