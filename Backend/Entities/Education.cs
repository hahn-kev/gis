using System;
using LinqToDB;
using LinqToDB.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Backend.Entities
{
    public class Education : BaseEntity
    {
        public Guid PersonId { get; set; }
        [Column(DataType = DataType.VarChar)]
        public Degree Degree { get; set; }
        public string Field { get; set; }
        public string Institution { get; set; }
        public string Country { get; set; }
        public DateTime CompletedDate { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Degree
    {
        [MapValue(nameof(Diploma))]
        Diploma,

        [MapValue(nameof(Associates))]
        Associates,

        [MapValue(nameof(Bachelors))]
        Bachelors,

        [MapValue(nameof(PGCE))]
        PGCE,

        [MapValue(nameof(Masters))]
        Masters,

        [MapValue(nameof(Doctorate))]
        Doctorate,

        [MapValue(nameof(Other))]
        Other
    }
}