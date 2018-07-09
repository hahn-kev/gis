using System;
using LinqToDB.Identity;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    [Table("User", IsColumnAttributeRequired = false)]
    public class IdentityUser: IdentityUser<int>, IUser
    {
        public bool ResetPassword { get; set; }
        public Guid? PersonId { get; set; }
        public bool SendHrLeaveEmails { get; set; }
    }
}