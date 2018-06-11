using System.Collections.Generic;

namespace Backend.Entities
{
    public class PersonAndLeaveDetails
    {
        public PersonWithStaff Person { get; set; }
        public IList<LeaveUsage> LeaveUsages { get; set; }
    }
}