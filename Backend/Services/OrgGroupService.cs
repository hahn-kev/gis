﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.DataLayer;
using Backend.Entities;
using LinqToDB;
using LinqToDB.Tools;

namespace Backend.Services
{
    public class OrgGroupService
    {
        private readonly OrgGroupRepository _orgGroupRepository;
        private readonly JobRepository _jobRepository;
        private readonly IEntityService _entityService;
        private readonly PersonRepository _personRepository;

        public OrgGroupService(OrgGroupRepository orgGroupRepository,
            IEntityService entityService,
            JobRepository jobRepository,
            PersonRepository personRepository)
        {
            _orgGroupRepository = orgGroupRepository;
            _entityService = entityService;
            _jobRepository = jobRepository;
            _personRepository = personRepository;
        }

        public List<OrgGroupWithSupervisor> OrgGroups =>
            _orgGroupRepository.OrgGroupsWithSupervisor.OrderBy(group => group.GroupName).ToList();

        public OrgGroup GetById(Guid id)
        {
            return _orgGroupRepository.OrgGroups.FirstOrDefault(group => group.Id == id);
        }

        public void Save(OrgGroup orgGroup)
        {
            _entityService.Save(orgGroup);
        }

        public void Delete(Guid id)
        {
            var staffUnderGroup = _personRepository.StaffWithNames
                .Where(staff => staff.OrgGroupId == id)
                .Select(staff => $"{staff.PreferredName} {staff.LastName}")
                .ToList();
            if (staffUnderGroup.Count > 0)
                throw new UserError("This Org still has the following users reporting to it: " +
                                    string.Join(", ", staffUnderGroup));
            _orgGroupRepository.OrgGroups.Where(child => child.ParentId == id)
                .Set(child => child.ParentId,
                    () => _orgGroupRepository.OrgGroups.Where(group => group.Id == id).Select(group => group.ParentId)
                        .SingleOrDefault()).Update();
            _entityService.Delete<OrgGroup>(id);
        }

        public OrgTreeData OrgTreeData(Guid? groupId = null)
        {
            var orgGroups = groupId.HasValue
                ? _orgGroupRepository.GetByIdWithChildren(groupId.Value)
                : _orgGroupRepository.OrgGroups;

            var jobs = from job in _jobRepository.Job
                from orgGroup in orgGroups.InnerJoin(g => g.Id == job.OrgGroupId)
                select job;
            var data = new OrgTreeData
            {
                Groups = (from orgGroup in _orgGroupRepository.OrgGroupsWithSupervisor
                    where orgGroup.Id.In(orgGroups.Select(g => g.Id))
                    select orgGroup).ToList(),
                Jobs = jobs.ToList(),
                Roles = (from role in _jobRepository.PersonRolesExtended
                    from job in jobs.InnerJoin(job => job.Id == role.JobId)
                    select role).ToList()
            };

            return data;
        }

        public Task<bool> IsPersonInGroup(Guid personId, Guid groupId)
        {
            return _orgGroupRepository.GetWithParentsWhere(group =>
                group.Id == _personRepository.PeopleWithStaffBasic
                    .Where(person => person.Id == personId)
                    .Select(basic => basic.Staff.OrgGroupId)
                    .Single()
            ).AnyAsync(group => group.Id == groupId);
        }

        public Task<bool> IsStaffInGroup(Guid staffId, Guid groupId)
        {
            return _orgGroupRepository.GetWithParentsWhere(group =>
                group.Id == _personRepository.Staff
                    .Where(staff => staff.Id == staffId)
                    .Select(staff => staff.OrgGroupId)
                    .Single()
            ).AnyAsync(group => group.Id == groupId);
        }

        public enum SortedBy
        {
            ParentFirst,
            ChildFirst,
            Either
        }

        public static bool IsOrgGroupSortedByHierarchy<T>(ICollection<T> orgGroups, SortedBy sortedBy = SortedBy.Either)
            where T : OrgGroup
        {
            if (orgGroups.Count <= 1) return true;

            bool ParentInList(OrgGroup group)
            {
                return orgGroups.Any(orgGroup => orgGroup.Id == @group.ParentId);
            }

            var groupWithoutParent = orgGroups.FirstOrDefault(group => !ParentInList(@group));
            var startsWithChild = orgGroups.First() != groupWithoutParent;
            switch (sortedBy)
            {
                case SortedBy.ChildFirst:
                    if (!startsWithChild) return false;
                    break;
                case SortedBy.ParentFirst:
                    if (startsWithChild) return false;
                    break;
            }

            OrgGroup previous = null;
            foreach (var current in startsWithChild ? orgGroups.Reverse() : orgGroups)
            {
                if (previous != null && current.ParentId != previous.Id)
                {
                    return false;
                }

                previous = current;
            }

            return true;
        }

        public static IEnumerable<T> SortOrgGroupByHierarchy<T>(IEnumerable<T> orgGroupsEnumerable,
            SortedBy sortedBy) where T : OrgGroup
        {
            var orgGroups = orgGroupsEnumerable.ToArray();

            OrgGroup Find(Guid? id)
            {
                if (id == null) return null;
                return orgGroups.FirstOrDefault(group => group.Id == id.Value);
            }

            int NumberOfParents(OrgGroup group)
            {
                var parent = Find(group.ParentId);
                if (parent == null) return 0;
                int count = 0;
                while (parent != null && parent != group)
                {
                    count++;
                    parent = Find(parent.ParentId);
                }

                return count;
            }

            var valueTuples = orgGroups.Select(group => (@group, parentCount: NumberOfParents(@group)));
            switch (sortedBy)
            {
                case SortedBy.Either:
                case SortedBy.ChildFirst:
                    valueTuples = valueTuples.OrderByDescending(tuple => tuple.parentCount)
                        .ThenBy(tuple => tuple.group.GroupName);
                    break;
                case SortedBy.ParentFirst:
                    valueTuples = valueTuples.OrderBy(tuple => tuple.parentCount)
                        .ThenBy(tuple => tuple.group.GroupName);
                    break;
            }

            return valueTuples.Select(tuple => tuple.group);
        }
    }
}