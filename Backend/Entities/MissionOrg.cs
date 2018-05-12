using System;
using System.Collections.Generic;
using LinqToDB;
using LinqToDB.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Backend.Entities
{
    public class MissionOrg : BaseEntity
    {
        public string Name { get; set; }
        public Guid RepId { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string AddressLocal { get; set; }
        public bool OfficeInThailand { get; set; }
        public DateTime? ApprovedDate { get; set; }

        [Column(DataType = DataType.VarChar)]
        public MissionOrgStatus? Status { get; set; }
    }

    public class MissionOrgWithNames : MissionOrg
    {
        public string ContactName { get; set; }
    }

    public class MissionOrgWithYearSummaries : MissionOrg
    {
        public IList<MissionOrgYearSummary> YearSummaries { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MissionOrgStatus
    {
        [MapValue(nameof(Associate))]
        Associate,

        [MapValue(nameof(OwnerAssociate))]
        OwnerAssociate,

        [MapValue("FounderAssociate")]
        [MapValue(nameof(FoundingAssociate))]
        FoundingAssociate,

        [MapValue(nameof(Founder))]
        Founder
    }

    public class MissionOrgYearSummary : BaseEntity
    {
        public Guid MissionOrgId { get; set; }
        public int Year { get; set; }
        public int StudentCount { get; set; }
        public int TeacherCount { get; set; }

        [Column(DataType = DataType.VarChar)]
        public MissionOrgStatus? Status { get; set; }

        [Column(DataType = DataType.VarChar)]
        public MissionOrgLevel? Level { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MissionOrgLevel
    {
        [MapValue(nameof(Bronze))]
        Bronze,

        [MapValue(nameof(Silver))]
        Silver,

        [MapValue(nameof(Gold))]
        Gold,

        [MapValue(nameof(Platinum))]
        Platinum
    }
}