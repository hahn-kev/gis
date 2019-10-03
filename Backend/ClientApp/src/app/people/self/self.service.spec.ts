import { inject, TestBed } from '@angular/core/testing';

import { SelfService } from './self.service';

xdescribe('SelfService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [SelfService]
    });
  });

  it('should be created', inject([SelfService], (service: SelfService) => {
    expect(service).toBeTruthy();
  }));
});
