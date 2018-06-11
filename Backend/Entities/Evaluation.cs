using System;
using LinqToDB;
using LinqToDB.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Backend.Entities
{
    public class Evaluation : BaseEntity
    {
        public Guid PersonId { get; set; }
        public Guid RoleId { get; set; }
        public Guid Evaluator { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public decimal Score { get; set; }
        public decimal Total { get; set; }
        [Column(DataType = DataType.VarChar)]
        public EvaluationResult Result { get; set; }
    }

    public class EvaluationWithNames : Evaluation
    {
        public string JobTitle { get; set; }
        public string EvaluatorPreferredName { get; set; }
        public string EvaluatorLastName { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EvaluationResult
    {
        [MapValue(nameof(Poor))]
        Poor,
        [MapValue(nameof(Good))]
        Good,
        [MapValue("Excelent")]
        [MapValue(nameof(Excellent))]
        Excellent
    }
}