import { inject, TestBed } from '@angular/core/testing';

import { MyErrorHandlerService } from './my-error-handler.service';

describe('MyErrorHandlerService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [MyErrorHandlerService]
    });
  });

  it('should be created', inject([MyErrorHandlerService], (service: MyErrorHandlerService) => {
    expect(service).toBeTruthy();
  }));
});
