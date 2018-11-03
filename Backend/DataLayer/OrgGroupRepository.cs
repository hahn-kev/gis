﻿using System;
using System.Linq;
using System.Linq.Expressions;
using Backend.Entities;
using LinqToDB;

namespace Backend.DataLayer
{
    public class OrgGroupRepository
    {
        private readonly IDbConnection _connection;
        private readonly PersonRepository _personRepository;

        public OrgGroupRepository(IDbConnection connection, PersonRepository personRepository)
        {
            _connection = connection;
            _personRepository = personRepository;
        }

        public IQueryable<OrgGroup> OrgGroups => _connection.OrgGroups;

        public IQueryable<OrgGroup> GetByIdWithChildren(Guid id)
        {
            return GetWithChildrenWhere(orgGroup => orgGroup.Id == id);
        }

        public IQueryable<OrgGroup> GetBySupervisorIdWithChildren(Guid supervisorId)
        {
            return GetWithChildrenWhere(orgGroup => orgGroup.Supervisor == supervisorId,
                childGroup => !childGroup.ApproverIsSupervisor);
        }

        public IQueryable<OrgGroup> GetWithChildrenWhere(Expression<Func<OrgGroup, bool>> rootPredicate,
            Expression<Func<OrgGroup, bool>> childPredicate = null)
        {
            return _connection.GetCte<OrgGroup>(parents =>
            {
                var rootQuery = (
                    from orgGroup in OrgGroups
                    select orgGroup
                ).Where(rootPredicate);
                var childQuery = (
                    from orgGroup in OrgGroups
                    from parent in parents.InnerJoin(parent => parent.Id == orgGroup.ParentId)
                    select orgGroup
                );
                if (childPredicate != null)
                    childQuery = childQuery.Where(childPredicate);
                return rootQuery.Union(childQuery);
            });
        }

        public IQueryable<OrgGroup> GetWithParentsWhere(Expression<Func<OrgGroup, bool>> rootPredicate, Func<IQueryable<OrgGroup>, IQueryable<OrgGroup>> visit = null)
        {
            return _connection.GetCte<OrgGroup>(children =>
            {
                var rootQuery = (
                    from orgGroup in OrgGroups
                    select orgGroup
                ).Where(rootPredicate);
                var childQuery = from orgGroup in OrgGroups
                    from parent in children.InnerJoin(child => child.ParentId== orgGroup.Id)
                    select orgGroup;
                if (visit != null)
                    childQuery = visit(childQuery);
                return rootQuery.Union(childQuery);
            });
        }

        public IQueryable<OrgGroupWithSupervisor> OrgGroupsWithSupervisor =>
            from orgGroup in OrgGroups
            from person in _personRepository.PeopleWithStaff
                .Where(staff => staff.StaffId != null)
                .LeftJoin(person => person.Id == orgGroup.Supervisor)
                .DefaultIfEmpty()
            select new OrgGroupWithSupervisor
            {
                ApproverIsSupervisor = orgGroup.ApproverIsSupervisor,
                GroupName = orgGroup.GroupName,
                Id = orgGroup.Id,
                Supervisor = orgGroup.Supervisor,
                ParentId = orgGroup.ParentId,
                Type = orgGroup.Type,
                SupervisorPerson = person
            };

        public (PersonWithStaff personOnLeave,
            OrgGroupWithSupervisor department,
            OrgGroupWithSupervisor devision,
            OrgGroupWithSupervisor supervisorGroup) PersonWithOrgGroupChain(Guid personId)
        {
            var result =
                (from personOnLeave in _personRepository.PeopleWithStaff.Where(person => person.Id == personId)
                    from department in OrgGroupsWithSupervisor.LeftJoin(@group =>
                            @group.Id == personOnLeave.Staff.OrgGroupId || @group.Supervisor == personOnLeave.Id)
                        .DefaultIfEmpty()
                    from devision in OrgGroupsWithSupervisor.LeftJoin(@group => @group.Id == department.ParentId)
                        .DefaultIfEmpty()
                    from supervisorGroup in OrgGroupsWithSupervisor.LeftJoin(@group => @group.Id == devision.ParentId)
                        .DefaultIfEmpty()
                    select new
                    {
                        personOnLeave,
                        department,
                        devision,
                        supervisorGroup
                    }).FirstOrDefault();
            return (result?.personOnLeave, result?.department, result?.devision, result?.supervisorGroup);
        }
    }
}