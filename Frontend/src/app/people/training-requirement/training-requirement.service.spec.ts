import { inject, TestBed } from '@angular/core/testing';

import { TrainingRequirementService } from './training-requirement.service';

describe('TrainingRequirementService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [TrainingRequirementService]
    });
  });

  it('should be created', inject([TrainingRequirementService], (service: TrainingRequirementService) => {
    expect(service).toBeTruthy();
  }));
});
