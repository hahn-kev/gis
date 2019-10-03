import { inject, TestBed } from '@angular/core/testing';

import { RoleGuardService } from './role-guard.service';

xdescribe('RoleGuardService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [RoleGuardService]
    });
  });

  it('should be created', inject([RoleGuardService], (service: RoleGuardService) => {
    expect(service).toBeTruthy();
  }));
});
