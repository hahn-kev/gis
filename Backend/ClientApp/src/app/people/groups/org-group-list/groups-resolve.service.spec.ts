import { inject, TestBed } from '@angular/core/testing';

import { GroupsResolveService } from './groups-resolve.service';

xdescribe('GroupsResolveService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [GroupsResolveService]
    });
  });

  it('should be created', inject([GroupsResolveService], (service: GroupsResolveService) => {
    expect(service).toBeTruthy();
  }));
});
