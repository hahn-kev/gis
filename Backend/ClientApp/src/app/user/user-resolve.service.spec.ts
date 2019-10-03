import { inject, TestBed } from '@angular/core/testing';

import { UserResolveService } from './user-resolve.service';

xdescribe('UserResolveService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [UserResolveService]
    });
  });

  it('should be created', inject([UserResolveService], (service: UserResolveService) => {
    expect(service).toBeTruthy();
  }));
});
