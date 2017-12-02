import { TestBed, inject } from '@angular/core/testing';

import { GroupsResolveService } from './groups-resolve.service';

describe('GroupsResolveService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [GroupsResolveService]
    });
  });

  it('should be created', inject([GroupsResolveService], (service: GroupsResolveService) => {
    expect(service).toBeTruthy();
  }));
});
