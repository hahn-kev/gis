import { inject, TestBed } from '@angular/core/testing';

import { JobResolverService } from './job-resolver.service';

xdescribe('JobResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [JobResolverService]
    });
  });

  it('should be created', inject([JobResolverService], (service: JobResolverService) => {
    expect(service).toBeTruthy();
  }));
});
