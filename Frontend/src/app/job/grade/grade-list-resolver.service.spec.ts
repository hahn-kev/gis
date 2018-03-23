import { TestBed, inject } from '@angular/core/testing';

import { GradeListResolverService } from './grade-list-resolver.service';

describe('GradeListResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [GradeListResolverService]
    });
  });

  it('should be created', inject([GradeListResolverService], (service: GradeListResolverService) => {
    expect(service).toBeTruthy();
  }));
});
