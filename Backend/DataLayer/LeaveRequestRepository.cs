using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Entities;
using LinqToDB;

namespace Backend.DataLayer
{
    public class LeaveRequestRepository
    {
        private readonly IDbConnection _connection;

        public LeaveRequestRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public IQueryable<LeaveRequest> LeaveRequests => _connection.LeaveRequests;

        public IQueryable<LeaveRequestWithNames> LeaveRequestWithNames =>
            from l in _connection.LeaveRequests
            from person in _connection.People.InnerJoin(person => person.Id == l.PersonId)
            from staff in _connection.Staff.LeftJoin(s => s.Id == person.StaffId).DefaultIfEmpty()
            from orgGroup in _connection.OrgGroups.LeftJoin(org => org.Id == staff.OrgGroupId).DefaultIfEmpty()
            from supervisor in _connection.People.LeftJoin(supervisor => supervisor.Id == l.ApprovedById)
                .DefaultIfEmpty()
            where !person.Deleted
            orderby l.StartDate descending
            select new LeaveRequestWithNames
            {
                Approved = l.Approved,
                ApprovedById = l.ApprovedById,
                ApprovedByName = (supervisor.PreferredName ?? supervisor.FirstName) + " " + supervisor.LastName,
                CreatedDate = l.CreatedDate,
                EndDate = l.EndDate,
                Id = l.Id,
                PersonId = l.PersonId,
                RequesterName = (person.PreferredName ?? person.FirstName) + " " + person.LastName,
                OrgGroupName = orgGroup.GroupName,
                OrgGroupId = orgGroup.Id,
                StartDate = l.StartDate,
                Type = l.Type,
                Reason = l.Reason,
                Days = l.Days,
                OverrideDays = l.OverrideDays
            };

        public List<LeaveRequestPublic> PublicLeaveRequests()
        {
            return (from request in LeaveRequestWithNames
                where request.Approved == true
                select new LeaveRequestPublic
                {
                    Id = request.Id,
                    RequesterName = request.RequesterName,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Days = request.Days,
                    OrgGroupName = request.OrgGroupName
                }).ToList();
        }

        public bool ApproveLeaveRequest(Guid id, Guid approver)
        {
            return _connection.LeaveRequests.Where(request => request.Id == id)
                       .Set(request => request.Approved, true)
                       .Set(request => request.ApprovedById, approver)
                       .Update() == 1;
        }
    }
}