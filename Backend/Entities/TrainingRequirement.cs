using System;
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