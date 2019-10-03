import { inject, TestBed } from '@angular/core/testing';

import { GradeService } from './grade.service';

xdescribe('GradeService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [GradeService]
    });
  });

  it('should be created', inject([GradeService], (service: GradeService) => {
    expect(service).toBeTruthy();
  }));
});
