import { inject, TestBed } from '@angular/core/testing';

import { OrgTreeDataResolverService } from './org-tree-data-resolver.service';

describe('OrgTreeDataResolverService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [OrgTreeDataResolverService]
    });
  });

  it('should be created', inject([OrgTreeDataResolverService], (service: OrgTreeDataResolverService) => {
    expect(service).toBeTruthy();
  }));
});
