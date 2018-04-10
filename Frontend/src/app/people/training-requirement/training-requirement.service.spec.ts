import { inject, TestBed } from '@angular/core/testing';

import { TrainingRequirementService } from './training-requirement.service';
import { HttpClientModule } from '@angular/common/http';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/of';
import { StaffWithName } from '../staff';
import { TrainingRequirement } from './training-requirement';
import { StaffTraining } from './staff-training';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { GroupService } from 'app/people/groups/group.service';

describe('TrainingRequirementService', () => {
  let groupServiceSpy = jasmine.createSpyObj<GroupService>('groupService', ['isChildOf']);
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientModule, HttpClientTestingModule],
      providers: [TrainingRequirementService,

        {provide: GroupService, useValue: groupServiceSpy}]
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
        Observable.of(2017),
        Observable.of(true))).not.toBeNull();
    }));

    it('should have a list of things', inject([TrainingRequirementService], (service: TrainingRequirementService) => {
      let spy = jasmine.createSpy('reqWStaff');
      service.buildRequirementsWithStaff(
        Observable.of([new StaffWithName('s1', 'Jim')]),
        Observable.of([new TrainingRequirement('tr1', 'fire', 2016)]),
        Observable.of(new Map([['s1', StaffTraining.New('s1', 'tr1')]])),
        Observable.of(2017),
        Observable.of(true))
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
        Observable.of(new Map([['s1', StaffTraining.New('s1','tr1')]])),
        year,
        Observable.of(true))
        .subscribe(spy);
      expect(spy.calls.count()).toBe(1);
      year.next(2016);
      expect(spy.calls.count()).toBe(2);
    }));
  });

  describe('isInYear', () => {
    it(`should be true if there's no end year`, inject([TrainingRequirementService], (service: TrainingRequirementService) => {
      let tr = new TrainingRequirement();
      tr.firstYear = 2016;
      tr.lastYear = null;
      expect(service.isInYear(2017, tr)).toBeTruthy();
    }));
    it(`should be true if it's the same as the first year`, inject([TrainingRequirementService], (service: TrainingRequirementService) => {
      let tr = new TrainingRequirement();
      tr.firstYear = 2017;
      tr.lastYear = 2019;
      expect(service.isInYear(2017, tr)).toBeTruthy();
    }));
    it(`should be false if it's before the first year`, inject([TrainingRequirementService], (service: TrainingRequirementService) => {
      let tr = new TrainingRequirement();
      tr.firstYear = 2017;
      tr.lastYear = 2019;
      expect(service.isInYear(2016, tr)).toBeFalsy();
    }));
    it(`should be false if it's after the last year`, inject([TrainingRequirementService], (service: TrainingRequirementService) => {
      let tr = new TrainingRequirement();
      tr.firstYear = 2017;
      tr.lastYear = 2019;
      expect(service.isInYear(2020, tr)).toBeFalsy();
    }));

  });


  describe('isInOrgGroup', () => {
    let service: TrainingRequirementService;
    beforeEach(inject([TrainingRequirementService], (s) => service = s));
    it('should let through all staff when type of requirement is all staff', () => {
      let tr = new TrainingRequirement();
      tr.scope = 'AllStaff';
      let staff = new StaffWithName();
      expect(service.isInOrgGroup([], tr, staff)).toBeTruthy();
    });
    it('should call the group service with department type', () => {
      let tr = new TrainingRequirement();
      tr.departmentId = 'd1';
      tr.scope = 'Department';
      let staff = new StaffWithName();
      staff.orgGroupId = 'd2';
      service.isInOrgGroup([], tr, staff);
      expect(groupServiceSpy.isChildOf.calls.any()).toBeTruthy();
    });
  });
});
