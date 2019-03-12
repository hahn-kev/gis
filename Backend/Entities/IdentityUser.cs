using System;
using LinqToDB.Mapping;
using Microsoft.AspNetCore.Identity;

namespace Backend.Entities
{
    [Table("User", IsColumnAttributeRequired = false)]
    public class IdentityUser : IdentityUser<int>, IUser
    {
        public bool ResetPassword { get; set; }
        public Guid? PersonId { get; set; }
        public bool SendHrLeaveEmails { get; set; }
    }
}