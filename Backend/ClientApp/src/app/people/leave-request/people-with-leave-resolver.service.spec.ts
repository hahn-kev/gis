import { inject, TestBed } from '@angular/core/testing';

import { PeopleWithLeaveResolverService } from './people-with-leave-resolver.service';

xdescribe('PeopleWithLeaveResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PeopleWithLeaveResolverService]
    });
  });

  it('should be created', inject([PeopleWithLeaveResolverService], (service: PeopleWithLeaveResolverService) => {
    expect(service).toBeTruthy();
  }));
});
