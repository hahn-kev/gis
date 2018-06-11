using LinqToDB;
using LinqToDB.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Backend.Entities
{
    public class Donor : BaseEntity
    {
        public string Notes { get; set; }

        [Column(DataType = DataType.VarChar)]
        public DonorStatus Status { get; set; }

        public bool IsBigDonor { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DonorStatus
    {
        [MapValue(nameof(Unknown))]
        Unknown,

        [MapValue(nameof(Active))]
        Active,

        [MapValue(nameof(Inactive))]
        Inactive
    }
}