import { TestBed, inject } from '@angular/core/testing';

import { GradeResolverService } from './grade-resolver.service';

describe('GradeResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [GradeResolverService]
    });
  });

  it('should be created', inject([GradeResolverService], (service: GradeResolverService) => {
    expect(service).toBeTruthy();
  }));
});
