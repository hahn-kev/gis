import { inject, TestBed } from '@angular/core/testing';

import { TrainingListResolverService } from './training-list-resolver.service';

describe('TrainingListResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [TrainingListResolverService]
    });
  });

  it('should be created', inject([TrainingListResolverService], (service: TrainingListResolverService) => {
    expect(service).toBeTruthy();
  }));
});
