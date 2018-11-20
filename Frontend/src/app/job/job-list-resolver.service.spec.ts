import { inject, TestBed } from '@angular/core/testing';

import { JobListResolverService } from './job-list-resolver.service';

xdescribe('JobListResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [JobListResolverService]
    });
  });

  it('should be created', inject([JobListResolverService], (service: JobListResolverService) => {
    expect(service).toBeTruthy();
  }));
});
