import { TestBed, inject } from '@angular/core/testing';

import { ActivityIndicatorInterceptorService } from './activity-indicator-interceptor.service';

describe('ActivityIndicatorInterceptorService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ActivityIndicatorInterceptorService]
    });
  });

  it('should be created', inject([ActivityIndicatorInterceptorService], (service: ActivityIndicatorInterceptorService) => {
    expect(service).toBeTruthy();
  }));
});
