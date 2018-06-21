import { inject, TestBed } from '@angular/core/testing';

import { PolicyGuard } from './policy.guard';

describe('PolicyGuard', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PolicyGuard]
    });
  });

  it('should ...', inject([PolicyGuard], (guard: PolicyGuard) => {
    expect(guard).toBeTruthy();
  }));
});
