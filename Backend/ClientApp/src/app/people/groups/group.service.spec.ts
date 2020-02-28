import { inject, TestBed } from '@angular/core/testing';

import { HttpClientModule } from '@angular/common/http';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { GroupService } from './group.service';
import { OrgGroup } from './org-group';
import { Person } from '../person';

describe('GroupService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientModule, HttpClientTestingModule],
      providers: [GroupService]
    });
  });

  it('should be created', inject([GroupService], (service: GroupService) => {
    expect(service).toBeTruthy();
  }));

  describe('isChildOf', () => {
    let service: GroupService;
    beforeEach(inject([GroupService], (s) => service = s));
    it('should be true if child and parent are same', () => {
      expect(service.isChildOf('1', '1', [])).toBeTruthy();
    });
    let newGroup = (id, parentId = null) => {
      let group = new OrgGroup();
      group.id = id;
      group.parentId = parentId;
      return group;
    };
    it('should be false when child is of another group', () => {
      let groups = new Map<string, OrgGroup>();
      groups.set('g1', newGroup('g1', 'gp2'));
      expect(service.isChildOf('g1', 'gp1', groups)).toBeFalsy();
    });
    it('should be true with a single child', () => {
      let groups = new Map<string, OrgGroup>();
      groups.set('g1', newGroup('g1', 'gp1'));
      expect(service.isChildOf('g1', 'gp1', groups)).toBeTruthy();
    });

    it('should be true with multiple children inbetween', () => {
      let groups = new Map<string, OrgGroup>();
      groups.set('g1', newGroup('g1', 'gp1'));
      groups.set('gp1', newGroup('gp1', 'gp2'));
      groups.set('gp2', newGroup('gp2', 'gp3'));
      groups.set('gp3', newGroup('gp3', 'gp4'));
      groups.set('gp4', newGroup('gp4', 'gp5'));
      expect(service.isChildOf('g1', 'gp5', groups)).toBeTruthy();
    });

    it('should work with a list instead of a map', () => {
      let groups = [newGroup('g1', 'gp1')];
      expect(service.isChildOf('g1', 'gp1', groups)).toBeTruthy();
    });

    it('should error on loops', () => {
      let group1 = newGroup('g1', 'g2');
      let group2 = newGroup('g2', 'g3');
      let group3 = newGroup('g3', 'g1');
      expect(() => service.isChildOf('g1', 'g4', [group1, group2, group3, newGroup('g4')])).toThrowError();
    });
  });
  describe('buildOrgChain', () => {
    let service: GroupService;
    beforeEach(inject([GroupService], (s) => service = s));

    let newGroup = (id, parentId = null, supervisor = null, approverIsSupervisor = true) => {
      let group = new OrgGroup();
      group.id = id;
      group.parentId = parentId;
      group.supervisor = supervisor;
      group.approverIsSupervisor = approverIsSupervisor;
      return group;
    };
    let newPerson = (id) => {
      let person = new Person();
      person.id = id;
      return person;
    };
    it('should build a short chain', () => {
      let group = newGroup('g1', 'g2');
      let orgChain = service.buildOrgChain(group, [newPerson('p1')], [newGroup('g2', null, 'p1')]);
      expect(orgChain.links.length).toBe(3);
      expect(orgChain.linkEnd.id).toBe('p1');
    });
    it('should skip the main group if supervisor is current person', () => {
      let group = newGroup('g1', 'g2', 'p2');
      let orgChain = service.buildOrgChain(group,
        [newPerson('p1'), newPerson('p2')],
        [newGroup('g2', null, 'p1')], 'p2');
      expect(orgChain.links.length).toBe(3);
      expect(orgChain.linkEnd.id).toBe('p1');
    });
    it('should skip the non approver groups', () => {
      let group1 = newGroup('g1', 'g2', null);
      let group2 = newGroup('g2', 'g3', 'p2', false);
      let group3 = newGroup('g3', null, 'p3', true);
      let orgChain = service.buildOrgChain(group1,
        [newPerson('p1'), newPerson('p2'), newPerson('p3')],
        [group1, group2, group3], 'p1');
      expect(orgChain.links.length).toBe(4);
      expect(orgChain.linkEnd.id).toBe('p3');
    });
    it('should throw if a loop is found', () => {
      let group1 = newGroup('g1', 'g2');
      let group2 = newGroup('g2', 'g3');
      let group3 = newGroup('g3', 'g1');
      expect(() => service.buildOrgChain(group1, [], [group1, group2, group3])).toThrowError();
    });
    it('should work when it has no id', () => {
      let group = newGroup(null);
      let orgChain = service.buildOrgChain(group, [newPerson('p1')], [newGroup('g2')], null);
      expect(orgChain.links.length).toBe(0);
    });
    it('should handle case where no person is found', () => {
      let group = newGroup('g1', 'g2');
      let orgChain = service.buildOrgChain(group, [newPerson('p1')], [newGroup('g2')]);
      expect(orgChain.links.length).toBe(3);
      expect(orgChain.linkEnd.id).toBe('');
      expect(orgChain.linkEnd.title).toBe('No One');
    });
  });
});
