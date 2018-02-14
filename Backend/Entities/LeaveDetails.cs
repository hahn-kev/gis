using System.Collections.Generic;

namespace Backend.Entities
{
    public class LeaveDetails
    {
        public IList<LeaveUseage> LeaveUseages { get; set; }
    }

    public class LeaveUseage
    {
        public LeaveType LeaveType { get; set; }
        public int Used { get; set; }
        public int? TotalAllowed { get; set; }
        public int? Left => TotalAllowed - Used;
    }
}