import { inject, TestBed } from "@angular/core/testing";

import { IsNewResolverService } from "./is-new-resolver.service";

describe('IsNewResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [IsNewResolverService]
    });
  });

  it('should be created', inject([IsNewResolverService], (service: IsNewResolverService) => {
    expect(service).toBeTruthy();
  }));
});
