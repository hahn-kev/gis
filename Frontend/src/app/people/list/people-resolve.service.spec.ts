import { TestBed, inject } from '@angular/core/testing';

import { PeopleResolveService } from './people-resolve.service';

describe('PeopleResolveService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PeopleResolveService]
    });
  });

  it('should be created', inject([PeopleResolveService], (service: PeopleResolveService) => {
    expect(service).toBeTruthy();
  }));
});
