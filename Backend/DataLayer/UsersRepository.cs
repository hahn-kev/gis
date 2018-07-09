using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Entities;
using LinqToDB;

namespace Backend.DataLayer
{
    public class UsersRepository
    {
        private readonly IDbConnection _dbConnection;

        public UsersRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public IQueryable<UserProfile> Users =>
            from user in _dbConnection.Users
            from person in _dbConnection.People.LeftJoin(p => p.Id == user.PersonId).DefaultIfEmpty()
            select new UserProfile
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,
                PersonId = user.PersonId,
                ResetPassword = user.ResetPassword,
                PersonName = (person.PreferredName ?? person.FirstName) + " " + person.LastName,
                SendHrLeaveEmails = user.SendHrLeaveEmails
            };

        public UserProfile UserByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return FillRoles(Users.FirstOrDefault(user =>
                user.UserName == name));
        }

        public void UpdatePersonId(int id, Guid personId)
        {
            _dbConnection.Users.Where(user => user.Id == id).Set(user => user.PersonId, personId).Update();
        }

        public UserProfile UserById(int id) => FillRoles(Users.FirstOrDefault(user => user.Id == id));

        private UserProfile FillRoles(UserProfile profile)
        {
            profile.Roles = (from userRole in _dbConnection.UserRoles
                from role in _dbConnection.Roles.LeftJoin(role => role.Id == userRole.RoleId)
                where userRole.UserId == profile.Id
                select role.Name).ToList();
            return profile;
        }

        public void DeleteUser(int id)
        {
            using (var dataConnectionTransaction = _dbConnection.BeginTransaction())
            {
                _dbConnection.UserClaims.Where(claim => claim.UserId == id).Delete();
                _dbConnection.UserLogins.Where(login => login.UserId == id).Delete();
                _dbConnection.UserRoles.Where(role => role.UserId == id).Delete();
                _dbConnection.UserTokens.Where(token => token.UserId == id).Delete();
                _dbConnection.Users.Where(user => user.Id == id).Delete();
                dataConnectionTransaction.Commit();
            }
        }
    }
}