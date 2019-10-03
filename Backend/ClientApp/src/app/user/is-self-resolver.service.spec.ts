import { inject, TestBed } from '@angular/core/testing';

import { IsSelfResolverService } from './is-self-resolver.service';

describe('IsSelfResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [IsSelfResolverService]
    });
  });

  it('should be created', inject([IsSelfResolverService], (service: IsSelfResolverService) => {
    expect(service).toBeTruthy();
  }));
});
