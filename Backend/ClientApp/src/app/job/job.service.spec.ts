import { inject, TestBed } from '@angular/core/testing';

import { JobService } from './job.service';

xdescribe('JobService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [JobService]
    });
  });

  it('should be created', inject([JobService], (service: JobService) => {
    expect(service).toBeTruthy();
  }));
});
