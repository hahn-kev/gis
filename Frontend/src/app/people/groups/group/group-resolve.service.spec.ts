import { inject, TestBed } from '@angular/core/testing';

import { GroupResolveService } from './group-resolve.service';

xdescribe('GroupResolveService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [GroupResolveService]
    });
  });

  it('should be created', inject([GroupResolveService], (service: GroupResolveService) => {
    expect(service).toBeTruthy();
  }));
});
