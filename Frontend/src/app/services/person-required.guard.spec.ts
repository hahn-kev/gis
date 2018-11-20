import { inject, TestBed } from '@angular/core/testing';

import { PersonRequiredGuard } from './person-required.guard';

xdescribe('PersonRequiredGuard', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PersonRequiredGuard]
    });
  });

  it('should ...', inject([PersonRequiredGuard], (guard: PersonRequiredGuard) => {
    expect(guard).toBeTruthy();
  }));
});
