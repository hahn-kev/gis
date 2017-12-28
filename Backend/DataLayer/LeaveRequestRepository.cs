using System;
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
            from person in _connection.PeopleExtended.InnerJoin(person => person.Id == l.PersonId)
            from supervisor in _connection.PeopleExtended.LeftJoin(supervisor => supervisor.Id == l.ApprovedById)
            select new LeaveRequestWithNames()
            {
                Approved = l.Approved,
                ApprovedById = l.ApprovedById,
                ApprovedByName = supervisor.PreferredName,
                CreatedDate = l.CreatedDate,
                EndDate = l.EndDate,
                Id = l.Id,
                PersonId = l.PersonId,
                RequesterName = person.PreferredName,
                StartDate = l.StartDate,
                Type = l.Type
            };

        public bool ApproveLeaveRequest(Guid id, Guid approver)
        {
            return _connection.LeaveRequests.Where(request => request.Id == id)
                       .Set(request => request.Approved, true)
                       .Set(request => request.ApprovedById, approver)
                       .Update() == 1;
        }
    }
}