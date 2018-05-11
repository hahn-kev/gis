using System;
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
}