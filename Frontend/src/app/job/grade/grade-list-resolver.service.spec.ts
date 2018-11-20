import { inject, TestBed } from '@angular/core/testing';

import { GradeListResolverService } from './grade-list-resolver.service';

xdescribe('GradeListResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [GradeListResolverService]
    });
  });

  it('should be created', inject([GradeListResolverService], (service: GradeListResolverService) => {
    expect(service).toBeTruthy();
  }));
});
