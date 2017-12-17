import { inject, async, TestBed } from '@angular/core/testing';

import { TrainingRequirementService } from './training-requirement.service';
import { HttpClientModule } from '@angular/common/http';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/of';
import { StaffWithName } from '../person';
import { TrainingRequirement } from './training-requirement';
import { StaffTraining } from './staff-training';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

describe('TrainingRequirementService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientModule, HttpClientTestingModule],
      providers: [TrainingRequirementService]
    });
  });

  it('should be created', inject([TrainingRequirementService], (service: TrainingRequirementService) => {
    expect(service).toBeTruthy();
  }));

  describe('buildRequirementsWithStaff', () => {
    it('should not return null', inject([TrainingRequirementService], (service: TrainingRequirementService) => {
      expect(service.buildRequirementsWithStaff(Observable.of([]),
        Observable.of([]),
        Observable.of(new Map()),
        Observable.of(2017))).not.toBeNull();
    }));

    it('should have a list of things', inject([TrainingRequirementService], (service: TrainingRequirementService) => {
      let spy = jasmine.createSpy('reqWStaff');
      service.buildRequirementsWithStaff(
        Observable.of([new StaffWithName('s1', 'Jim')]),
        Observable.of([new TrainingRequirement('tr1', 'fire', 2016)]),
        Observable.of(new Map([['s1', new StaffTraining('s1', 'tr1')]])),
        Observable.of(2017))
        .subscribe(spy);
      expect(spy.calls.any()).toBeTruthy();
      expect(spy.calls.mostRecent().args[0].length).toBe(1);
    }));

    it('should result whenever an observable is updated', inject([TrainingRequirementService], (service: TrainingRequirementService) => {
      let spy = jasmine.createSpy('reqWStaff');
      let year = new BehaviorSubject(2017);
      service.buildRequirementsWithStaff(
        Observable.of([new StaffWithName('s1', 'Jim')]),
        Observable.of([new TrainingRequirement('tr1', 'fire', 2016)]),
        Observable.of(new Map([['s1', new StaffTraining('s1', 'tr1')]])),
        year)
        .subscribe(spy);
      expect(spy.calls.count()).toBe(1);
      year.next(2016);
      expect(spy.calls.count()).toBe(2);
    }));
  });
});
