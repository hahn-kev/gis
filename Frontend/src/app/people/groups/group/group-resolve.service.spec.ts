import { TestBed, inject } from '@angular/core/testing';

import { GroupResolveService } from './group-resolve.service';

describe('GroupResolveService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [GroupResolveService]
    });
  });

  it('should be created', inject([GroupResolveService], (service: GroupResolveService) => {
    expect(service).toBeTruthy();
  }));
});
