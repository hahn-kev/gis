using System;
using System.Collections.Generic;
using LinqToDB;
using LinqToDB.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Backend.Entities
{
    public class Job : BaseEntity
    {
        public string Title { get; set; }
        [Column(DataType = DataType.VarChar)] public JobType Type { get; set; }
        public string JobDescription { get; set; }
        public Guid? GradeId { get; set; }
        public Guid OrgGroupId { get; set; }
        public bool Current { get; set; }
        public int Positions { get; set; }
    }

    public class JobWithFilledInfo : Job
    {
        public int Filled { get; set; }
        public int Open => Positions - Filled;
    }

    public class JobWithOrgGroup : Job
    {
        public OrgGroup OrgGroup { get; set; }
    }

    public class JobWithRoles : Job
    {
        public IList<PersonRoleExtended> Roles { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum JobType
    {
        FullTime,
        HalfTime,
        Contractor,
        DailyWorker,
        SchoolAid,
        FullTime10Mo,
    }
}