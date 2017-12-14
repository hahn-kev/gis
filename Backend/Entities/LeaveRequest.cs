using System;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    public class LeaveRequest : BaseEntity
    {
        public Guid PersonId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool? Approved { get; set; }
        public Guid? ApprovedById { get; set; }
        public string Type { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    [Table("LeaveRequest", IsColumnAttributeRequired = false)]
    public class LeaveRequestWithNames : LeaveRequest
    {
        [Column(SkipOnInsert = true, SkipOnUpdate = true, IsColumn = false)]
        public string RequesterName { get; set; }
        [Column(SkipOnInsert = true, SkipOnUpdate = true, IsColumn = false)]
        public string ApprovedByName { get; set; }
    }
}