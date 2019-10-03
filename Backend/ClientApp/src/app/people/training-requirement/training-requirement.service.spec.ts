import { fakeAsync, inject, TestBed, tick } from '@angular/core/testing';

import { TrainingRequirementService } from './training-requirement.service';
import { HttpClientModule } from '@angular/common/http';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { BehaviorSubject, of } from 'rxjs';
import { StaffWithName, StaffWithRoles } from '../staff';
import { TrainingRequirement, TrainingScope } from './training-requirement';
import { StaffTraining } from './staff-training';
import { GroupService } from 'app/people/groups/group.service';
import { OrgGroupWithSupervisor } from '../groups/org-group';
import { StaffWithTraining } from './training-report/requirement-with-staff';

describe('TrainingRequirementService', () => {
  let groupServiceSpy = jasmine.createSpyObj<GroupService>('groupService', ['isChildOf']);
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientModule, HttpClientTestingModule],
      providers: [
        TrainingRequirementService,

        {provide: GroupService, useValue: groupServiceSpy}
      ]
    });
  });

  it('should be created', inject([TrainingRequirementService], (service: TrainingRequirementService) => {
    expect(service).toBeTruthy();
  }));

  describe('buildRequirementsWithStaff', () => {
    it('should not return null', inject([TrainingRequirementService], (service: TrainingRequirementService) => {
      expect(service.buildRequirementsWithStaff(
        of([]),
        of([]),
        of([]),
        of(new Map()),
        of(2017),
        of(true))).not.toBeNull();
    }));

    function newTr(id: string, name: string, year) {
      let tr = new TrainingRequirement();
      tr.id = id;
      tr.name = name;
      tr.firstYear = year;
      tr.scope = TrainingScope.AllStaff;
      return tr;
    }

    function newStaff(id: string, name: string) {
      let staff = new StaffWithRoles();
      staff.staffWithName = new StaffWithName(id, name);
      staff.personRolesWithJob = [];
      return staff;
    }

    it('should have a list of things', inject([TrainingRequirementService], (service: TrainingRequirementService) => {
      let spy = jasmine.createSpy('reqWStaff');
      service.buildRequirementsWithStaff(
        of([newStaff('s1', 'Jim')]),
        of([new OrgGroupWithSupervisor()]),
        of([newTr('tr1', 'fire', 2016)]),
        of(new Map([['s1', StaffTraining.New('s1', 'tr1')]])),
        of(2017),
        of(true))
        .subscribe(spy);
      expect(spy.calls.any()).toBeTruthy();
      expect(spy.calls.mostRecent().args[0].length).toBe(1);
    }));

    it('should result whenever an observable is updated',
      fakeAsync(inject([TrainingRequirementService], (service: TrainingRequirementService) => {
        let spy = jasmine.createSpy('reqWStaff');
        let year = new BehaviorSubject(2017);
        service.buildRequirementsWithStaff(
          of([newStaff('s1', 'Jim')]),
          of([new OrgGroupWithSupervisor()]),
          of([newTr('tr1', 'fire', 2016)]),
          of(new Map([['s1', StaffTraining.New('s1', 'tr1')]])),
          year,
          of(true))
          .subscribe(spy);
        tick(50);
        expect(spy.calls.count()).toBe(1);
        year.next(2016);
        tick(50);
        expect(spy.calls.count()).toBe(2);
      })));
  });

  describe('isInYear', () => {
    it(`should be true if there's no end year`,
      inject([TrainingRequirementService], (service: TrainingRequirementService) => {
        let tr = new TrainingRequirement();
        tr.firstYear = 2016;
        tr.lastYear = null;
        expect(service.isInYear(2017, tr)).toBeTruthy();
      }));
    it(`should be true if it's the same as the first year`,
      inject([TrainingRequirementService], (service: TrainingRequirementService) => {
        let tr = new TrainingRequirement();
        tr.firstYear = 2017;
        tr.lastYear = 2019;
        expect(service.isInYear(2017, tr)).toBeTruthy();
      }));
    it(`should be false if it's before the first year`,
      inject([TrainingRequirementService], (service: TrainingRequirementService) => {
        let tr = new TrainingRequirement();
        tr.firstYear = 2017;
        tr.lastYear = 2019;
        expect(service.isInYear(2016, tr)).toBeFalsy();
      }));
    it(`should be false if it's after the last year`,
      inject([TrainingRequirementService], (service: TrainingRequirementService) => {
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
      tr.scope = TrainingScope.AllStaff;
      let staff = new StaffWithTraining(new StaffWithRoles(), new StaffTraining());
      expect(service.isInOrgGroup([], tr, staff)).toBeTruthy();
    });
    it('should call the group service with department type', () => {
      let tr = new TrainingRequirement();
      tr.departmentId = 'd1';
      tr.scope = TrainingScope.Department;
      let staff = new StaffWithTraining(new StaffWithRoles(new StaffWithName(), []), new StaffTraining());
      staff.staff.orgGroupId = 'd2';
      service.isInOrgGroup([], tr, staff);
      expect(groupServiceSpy.isChildOf.calls.any()).toBeTruthy();
    });
  });
});
