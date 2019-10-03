import { inject, TestBed } from '@angular/core/testing';

import { JobFilledListResolverService } from './job-filled-list-resolver.service';

xdescribe('JobFilledListResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [JobFilledListResolverService]
    });
  });

  it('should be created', inject([JobFilledListResolverService], (service: JobFilledListResolverService) => {
    expect(service).toBeTruthy();
  }));
});
