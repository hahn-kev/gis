import { inject, TestBed } from "@angular/core/testing";

import { JwtHelperService } from "./jwt-helper.service";

describe('JwtHelperService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [JwtHelperService]
    });
  });

  it('should be created', inject([JwtHelperService], (service: JwtHelperService) => {
    expect(service).toBeTruthy();
  }));
});
