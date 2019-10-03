import { inject, TestBed } from '@angular/core/testing';

import { PeopleResolveService } from './people-resolve.service';

xdescribe('PeopleResolveService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PeopleResolveService]
    });
  });

  it('should be created', inject([PeopleResolveService], (service: PeopleResolveService) => {
    expect(service).toBeTruthy();
  }));
});
