import { inject, TestBed } from '@angular/core/testing';

import { EvaluationSummaryResolveService } from './evaluation-summary-resolve.service';
import { EvaluationService } from '../person/evaluation/evaluation.service';

describe('EvaluationSummaryResolveService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        EvaluationSummaryResolveService,
        {provide: EvaluationService, useValue: jasmine.createSpyObj('EvaluationService', ['getSummaries'])}
      ]
    });
  });

  it('should be created', inject([EvaluationSummaryResolveService], (service: EvaluationSummaryResolveService) => {
    expect(service).toBeTruthy();
  }));
});
