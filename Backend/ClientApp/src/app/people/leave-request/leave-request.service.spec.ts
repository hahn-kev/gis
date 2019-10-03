import { inject, TestBed } from '@angular/core/testing';

import { LeaveRequestService } from './leave-request.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('LeaveRequestService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule
      ],
      providers: [LeaveRequestService]
    });
  });

  it('should be created', inject([LeaveRequestService], (service: LeaveRequestService) => {
    expect(service).toBeTruthy();
  }));

  describe('weekDaysBetween', () => {
    let service: LeaveRequestService;
    beforeEach(inject([LeaveRequestService], (_: LeaveRequestService) => service = _));
    it('should handle string dates', () => {
      expect(service.weekDaysBetween('2018-02-20', '2018-02-22')).toBe(3);
    });

    it('should handle date objects', () => {
      expect(service.weekDaysBetween(new Date(2018, 2, 20), new Date(2018, 2, 23))).toBe(4);
    });
    //2018-2-23 is a friday, the 26th is the next monday
    it('should ignore the weekend', () => {
      expect(service.weekDaysBetween('2018-02-22', '2018-02-27')).toBe(4);
    });
    it('should count same day as 1 day', () => {
      expect(service.weekDaysBetween('2018-02-22', '2018-02-22')).toBe(1);
    });
    it('should not count saturday when ending on that day', () => {
      expect(service.weekDaysBetween('2018-02-23', '2018-02-24')).toBe(1);
    });
    it('should not count sunday when ending on that day', () => {
      expect(service.weekDaysBetween('2018-02-23', '2018-02-25')).toBe(1);
    });
    it('should not count friday to monday as 2', () => {
      expect(service.weekDaysBetween('2018-02-23', '2018-02-26')).toBe(2);
    });
  });
});
