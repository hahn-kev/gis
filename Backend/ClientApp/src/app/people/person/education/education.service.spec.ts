import { inject, TestBed } from '@angular/core/testing';

import { EducationService } from './education.service';

xdescribe('EducationService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [EducationService]
    });
  });

  it('should be created', inject([EducationService], (service: EducationService) => {
    expect(service).toBeTruthy();
  }));
});
