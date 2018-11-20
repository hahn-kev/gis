import { inject, TestBed } from '@angular/core/testing';

import { ActivityIndicatorInterceptorService } from './activity-indicator-interceptor.service';

xdescribe('ActivityIndicatorInterceptorService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ActivityIndicatorInterceptorService]
    });
  });

  it('should be created', inject([ActivityIndicatorInterceptorService], (service: ActivityIndicatorInterceptorService) => {
    expect(service).toBeTruthy();
  }));
});
