import { inject, TestBed } from '@angular/core/testing';

import { TrainingResolverService } from './training-resolver.service';

xdescribe('TrainingResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [TrainingResolverService]
    });
  });

  it('should be created', inject([TrainingResolverService], (service: TrainingResolverService) => {
    expect(service).toBeTruthy();
  }));
});
