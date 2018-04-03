import { TestBed, inject } from '@angular/core/testing';

import { JobFilledListResolverService } from './job-filled-list-resolver.service';

describe('JobFilledListResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [JobFilledListResolverService]
    });
  });

  it('should be created', inject([JobFilledListResolverService], (service: JobFilledListResolverService) => {
    expect(service).toBeTruthy();
  }));
});
