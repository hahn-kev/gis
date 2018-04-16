using System;
using System.Collections.Generic;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    [Table("User", IsColumnAttributeRequired = false)]
    public class UserProfile : IUser
    {
        public UserProfile()
        {
        }

        public UserProfile(IUser user)
        {
            this.CopyFrom(user);
        }

        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool ResetPassword { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsHr { get; set; }
        public bool IsHrAdmin { get; set; }
        public Guid? PersonId { get; set; }
        public string PersonName { get; set; }
    }

    public interface IUser
    {
        int Id { get; set; }
        string UserName { get; set; }
        string Email { get; set; }
        string PhoneNumber { get; set; }
        bool ResetPassword { get; set; }
        Guid? PersonId { get; set; }
    }

    public static class UserExtensions
    {
        public static T CopyFrom<T>(this T user, IUser from) where T : IUser
        {
            if (from.Id != 0 && user.Id == 0)
                user.Id = from.Id;
            user.UserName = from.UserName;
            user.Email = from.Email;
            user.PhoneNumber = from.PhoneNumber;
            user.ResetPassword = from.ResetPassword;
            user.PersonId = from.PersonId;
            return user;
        }
    }
}