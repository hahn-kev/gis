import { inject, TestBed } from '@angular/core/testing';

import { SchoolAidResolveService } from './school-aid-resolve.service';

xdescribe('SchoolAidResolveService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [SchoolAidResolveService]
    });
  });

  it('should be created', inject([SchoolAidResolveService], (service: SchoolAidResolveService) => {
    expect(service).toBeTruthy();
  }));
});
