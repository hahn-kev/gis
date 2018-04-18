import { TestBed, inject } from '@angular/core/testing';

import { SchoolAidResolveService } from './school-aid-resolve.service';

describe('SchoolAidResolveService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [SchoolAidResolveService]
    });
  });

  it('should be created', inject([SchoolAidResolveService], (service: SchoolAidResolveService) => {
    expect(service).toBeTruthy();
  }));
});
