import { TestBed, inject } from '@angular/core/testing';

import { RolesResolverService } from './roles-resolver.service';

describe('RolesResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [RolesResolverService]
    });
  });

  it('should be created', inject([RolesResolverService], (service: RolesResolverService) => {
    expect(service).toBeTruthy();
  }));
});
