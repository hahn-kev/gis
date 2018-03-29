using System;
using LinqToDB;
using LinqToDB.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Backend.Entities
{
    public class TrainingRequirement : BaseEntity
    {
        public string Name { get; set; }
        public int FirstYear { get; set; }
        public int? LastYear { get; set; }
        /// <summary>
        /// if count is -1 then it never needs to be renewed
        /// </summary>
        public int RenewMonthsCount { get; set; }

        [Column(DataType = DataType.VarChar)]
        public TrainingScope Scope { get; set; }
        public Guid? DepatmentId { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TrainingScope
    {
        [MapValue("AllStaff")]
        AllStaff,

        [MapValue("Department")]
        Department
    }
}