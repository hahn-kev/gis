using System;
using Backend.Utils;
using LinqToDB;
using LinqToDB.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Backend.Entities
{
    public class LeaveRequest : BaseEntity
    {
        [JsonConstructor]
        public LeaveRequest()
        {
        }

        public LeaveRequest(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
            Days = CalculateLength();
        }

        public Guid PersonId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Days { get; set; }
        public bool OverrideDays { get; set; }
        public string Reason { get; set; }
        /// <summary>
        /// Null means pending, true is approved, false is rejected
        /// </summary>
        public bool? Approved { get; set; }

        /// <summary>
        /// Id of the person who approved the leave request
        /// </summary>
        public Guid? ApprovedById { get; set; }

        [Column(DataType = DataType.VarChar)]
        public LeaveType Type { get; set; }

        public DateTime CreatedDate { get; set; }

        public int CalculateLength()
        {
            return StartDate.BusinessDaysUntil(EndDate);
        }

        public LeaveRequest Copy()
        {
            return (LeaveRequest) MemberwiseClone();
        }
    }

    [Table("LeaveRequest", IsColumnAttributeRequired = false)]
    public class LeaveRequestWithNames : LeaveRequest
    {
        [Column(SkipOnInsert = true, SkipOnUpdate = true, IsColumn = false)]
        public string RequesterName { get; set; }

        [Column(SkipOnInsert = true, SkipOnUpdate = true, IsColumn = false)]
        public string ApprovedByName { get; set; }
    }


    [JsonConverter(typeof(StringEnumConverter))]
    public enum LeaveType
    {
        [MapValue("Vacation")]
        Vacation,

        [MapValue("Sick")]
        Sick,

        [MapValue("Personal")]
        Personal,

        [MapValue("Maternity")]
        Maternity,

        [MapValue("Paternity")]
        Paternity,
        
        [MapValue("Emergency")]
        Emergency,

        [MapValue("SchoolRelated")]
        SchoolRelated,
        
        [MapValue(nameof(MissionRelated))]
        MissionRelated,

        [MapValue("Other")]
        Other,
    }
}