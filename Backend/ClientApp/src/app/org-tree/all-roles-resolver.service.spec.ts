import { inject, TestBed } from '@angular/core/testing';

import { AllRolesResolverService } from './all-roles-resolver.service';

xdescribe('AllRolesResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AllRolesResolverService]
    });
  });

  it('should be created', inject([AllRolesResolverService], (service: AllRolesResolverService) => {
    expect(service).toBeTruthy();
  }));
});
