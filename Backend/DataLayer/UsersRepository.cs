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
            from adminRole in _dbConnection.Roles.Where(role => role.Name == "admin").DefaultIfEmpty()
            from hrRole in _dbConnection.Roles.Where(role => role.Name == "hr").DefaultIfEmpty()
            from hrAdminRole in _dbConnection.Roles.Where(role => role.Name == "hradmin").DefaultIfEmpty()
            from userAdminRole in _dbConnection.UserRoles
                .Where(userRole => userRole.RoleId == adminRole.Id && userRole.UserId == user.Id)
                .DefaultIfEmpty()
            from userHrRole in _dbConnection.UserRoles
                .Where(userRole => userRole.RoleId == hrRole.Id && userRole.UserId == user.Id)
                .DefaultIfEmpty()
            from userHrAdminRole in _dbConnection.UserRoles
                .Where(userRole => userRole.RoleId == hrAdminRole.Id && userRole.UserId == user.Id)
                .DefaultIfEmpty()
            select new UserProfile
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,
                IsAdmin = userAdminRole != null,
                IsHr = userHrRole != null,
                IsHrAdmin = userHrAdminRole != null,
                PersonId = user.PersonId,
                ResetPassword = user.ResetPassword,
                PersonName = (person.PreferredName ?? person.FirstName) + " " + person.LastName
            };

        public UserProfile UserByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return Users.FirstOrDefault(user =>
                user.UserName == name);
        }

        public void UpdatePersonId(int id, Guid personId)
        {
            _dbConnection.Users.Where(user => user.Id == id).Set(user => user.PersonId, personId).Update();
        }

        public UserProfile UserById(int id) => Users.FirstOrDefault(user => user.Id == id);

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