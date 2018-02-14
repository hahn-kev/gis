using System;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    public class PersonRole : BaseEntity
    {
        public string Name { get; set; }
        public Guid PersonId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool Active { get; set; }
        public bool IsDirectorPosition { get; set; }
        public bool IsStaffPosition { get; set; }
    }

    [Table("PersonRole", IsColumnAttributeRequired = false)]
    public class PersonRoleExtended : PersonRole
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}