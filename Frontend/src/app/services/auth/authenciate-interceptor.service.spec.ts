import { inject, TestBed } from "@angular/core/testing";

import { AuthenciateInterceptorService } from "./authenciate-interceptor.service";

describe('AuthenciateInterceptorService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AuthenciateInterceptorService]
    });
  });

  it('should be created', inject([AuthenciateInterceptorService], (service: AuthenciateInterceptorService) => {
    expect(service).toBeTruthy();
  }));
});
