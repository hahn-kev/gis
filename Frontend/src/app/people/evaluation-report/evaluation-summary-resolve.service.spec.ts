import { inject, TestBed } from '@angular/core/testing';

import { EvaluationSummaryResolveService } from './evaluation-summary-resolve.service';

describe('EvaluationSummaryResolveService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [EvaluationSummaryResolveService]
    });
  });

  it('should be created', inject([EvaluationSummaryResolveService], (service: EvaluationSummaryResolveService) => {
    expect(service).toBeTruthy();
  }));
});
