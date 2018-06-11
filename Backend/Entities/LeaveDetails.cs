using System.Collections.Generic;

namespace Backend.Entities
{
    public class LeaveDetails
    {
        public IList<LeaveUsage> LeaveUsages { get; set; }
    }

    public class LeaveUsage
    {
        public LeaveType LeaveType { get; set; }
        public decimal Used { get; set; }
        public int TotalAllowed { get; set; }
        public decimal Left => TotalAllowed - Used;

        public override string ToString()
        {
            return $"{LeaveType} {Used}/{TotalAllowed}";
        }
    }
}