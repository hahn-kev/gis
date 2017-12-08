using System;
using System.Linq;
using Backend.Entities;
using LinqToDB;

namespace Backend.DataLayer
{
    public class LeaveRequestRepository
    {
        private readonly DbConnection _connection;

        public LeaveRequestRepository(DbConnection connection)
        {
            _connection = connection;
        }

        public IQueryable<LeaveRequest> LeaveRequests => _connection.LeaveRequests;

        public bool ApproveLeaveRequest(Guid id, Guid approver)
        {
            return _connection.LeaveRequests.Where(request => request.Id == id)
                .Set(request => request.Approved, true)
                .Set(request => request.ApprovedById, approver)
                .Update() == 1;
        }
        
        
    }
}