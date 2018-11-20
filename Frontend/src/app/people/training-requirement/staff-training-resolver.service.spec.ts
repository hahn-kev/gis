import { inject, TestBed } from '@angular/core/testing';

import { StaffTrainingResolverService } from './staff-training-resolver.service';

xdescribe('StaffTrainingResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [StaffTrainingResolverService]
    });
  });

  it('should be created', inject([StaffTrainingResolverService], (service: StaffTrainingResolverService) => {
    expect(service).toBeTruthy();
  }));
});
