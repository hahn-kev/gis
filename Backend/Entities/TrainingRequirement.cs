using System;
using LinqToDB;
using LinqToDB.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NpgsqlTypes;

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

        public Guid? DepartmentId { get; set; }
        public Guid? OwnerId { get; set; }
        public string Provider { get; set; }

        /// <summary>
        /// this is actually an array of <see cref="JobType"/>
        /// </summary>
        [Column(IsColumn = true, DbType = "varchar[]")]
        public string[] JobScope { get; set; }
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