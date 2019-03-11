import { TestBed } from '@angular/core/testing';

import { HolidayResolverService } from './holiday-resolver.service';

xdescribe('HolidayResolverService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: HolidayResolverService = TestBed.get(HolidayResolverService);
    expect(service).toBeTruthy();
  });
});
