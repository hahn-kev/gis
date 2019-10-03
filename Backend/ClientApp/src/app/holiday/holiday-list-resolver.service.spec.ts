import { TestBed } from '@angular/core/testing';

import { HolidayListResolverService } from './holiday-list-resolver.service';

xdescribe('HolidayListResolverService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: HolidayListResolverService = TestBed.get(HolidayListResolverService);
    expect(service).toBeTruthy();
  });
});
