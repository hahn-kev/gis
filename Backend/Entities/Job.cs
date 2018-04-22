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
        [Column(DataType = DataType.VarChar)] public JobType? Type { get; set; }
        [Column(DataType = DataType.VarChar)] public JobStatus? Status { get; set; }
        public string JobDescription { get; set; }
        public Guid? GradeId { get; set; }
        public Guid OrgGroupId { get; set; }
        public bool Current { get; set; }
        public int Positions { get; set; }
    }

    public class JobWithFilledInfo : Job
    {
        public long Filled { get; set; }
        public long Open => Positions - Filled;
        public int? GradeNo { get; set; }
        public string OrgGroupName { get; set; }
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
    public enum JobStatus
    {
        [MapValue(nameof(FullTime))] [MapValue("0")]
        FullTime,

        [MapValue(nameof(HalfTime))] [MapValue("1")]
        HalfTime,

        [MapValue(nameof(Contractor))] [MapValue("2")]
        Contractor,

        [MapValue(nameof(DailyWorker))] [MapValue("3")]
        DailyWorker,

        [MapValue(nameof(SchoolAid))] [MapValue("4")]
        SchoolAid,

        [MapValue(nameof(FullTime10Mo))] [MapValue("5")]
        FullTime10Mo,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum JobType
    {
        [MapValue(nameof(Admin))] Admin,
        [MapValue(nameof(Support))] Support,
        [MapValue(nameof(Teacher))] Teacher,
        [MapValue(nameof(BlueCollar))] BlueCollar
    }
}