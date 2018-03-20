using System;
using LinqToDB;
using LinqToDB.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Backend.Entities
{
    public class Job : BaseEntity
    {
        public string Title { get; set; }
        [Column(DataType = DataType.VarChar)]
        public JobType Type { get; set; }
        public string JobDescription { get; set; }
        public Guid OrgGroupId { get; set; }
        public bool Current { get; set; }
        public bool IsDirector { get; set; }
        public bool IsStaff { get; set; }
        public int Positions { get; set; }
    }   
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum JobType
    {
        FullTime,
        HalfTime, 
        Contractor,
        DailyWorker,
        SchoolAid
    }
}