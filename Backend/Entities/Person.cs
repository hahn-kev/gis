using System;
using System.Collections.Generic;
using LinqToDB;
using LinqToDB.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Backend.Entities
{
    public class Person : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid? StaffId { get; set; }
        public string Email { get; set; }
        public bool Deleted { get; set; }

        public Gender Gender { get; set; }
        public bool IsThai { get; set; }
        public bool IsSchoolAid { get; set; }

        private string _preferredName;

        public string PreferredName
        {
            get => _preferredName ?? FirstName;
            set => _preferredName = value;
        }

        public override string ToString()
        {
            return $"{nameof(FirstName)}: {FirstName}, {nameof(LastName)}: {LastName}";
        }
    }

    [Table("Person", IsColumnAttributeRequired = false)]
    public class PersonExtended : Person
    {
        public string ThaiFirstName { get; set; }
        public string ThaiLastName { get; set; }
        public bool SpeaksEnglish { get; set; }
        public string PhoneNumber { get; set; }
        public Guid? SpouseId { get; set; }

        [Column(DataType = DataType.VarChar)] public Nationality? Nationality { get; set; }

        public DateTime? Birthdate { get; set; }

        [Column(SkipOnInsert = true, SkipOnUpdate = true, IsColumn = false)]
        public bool SpouseChanged { get; private set; }


        public string PassportAddress { get; set; }
        public string PassportCity { get; set; }
        public string PassportState { get; set; }
        public string PassportZip { get; set; }
        public string PassportCountry { get; set; }

        public string ThaiAddress { get; set; }
        public string ThaiSoi { get; set; }
        public string ThaiMubaan { get; set; }
        public string ThaiTambon { get; set; }
        public string ThaiAmphur { get; set; }
        public string ThaiProvince { get; set; }
        public string ThaiZip { get; set; }
    }

    [Table("Person", IsColumnAttributeRequired = false)]
    public class PersonWithStaff : PersonExtended
    {
        public StaffWithOrgName Staff { get; set; }

        [Column(SkipOnInsert = true, SkipOnUpdate = true, IsColumn = false)]
        public string SpousePreferedName { get; set; }
    }

    [Table("Person", IsColumnAttributeRequired = false)]
    public class PersonWithOthers : PersonWithStaff
    {
        [Column(SkipOnInsert = true, SkipOnUpdate = true, IsColumn = false)]
        public LeaveDetails LeaveDetails { get; set; }

        public IList<PersonRoleWithJob> Roles { get; set; } = new List<PersonRoleWithJob>(0);
        public IList<EmergencyContactExtended> EmergencyContacts { get; set; } = new List<EmergencyContactExtended>(0);
        public IList<EvaluationWithNames> Evaluations { get; set; } = new List<EvaluationWithNames>(0);
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Gender
    {
        Male,
        Female
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Nationality
    {
        [MapValue(nameof(NorthAmerica))] NorthAmerica,

        [MapValue(nameof(CentralSouthAmerica))]
        CentralSouthAmerica,

        [MapValue(nameof(Africa))] Africa,

        [MapValue(nameof(MiddleEast))] MiddleEast,

        [MapValue(nameof(Europe))] Europe,

        [MapValue(nameof(CentralAsia))] CentralAsia,

        [MapValue(nameof(EastAsia))] EastAsia,

        [MapValue(nameof(Oceania))] Oceania
    }
}