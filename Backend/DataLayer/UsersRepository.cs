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
            from role in _dbConnection.Roles.Where(role => role.Name == "admin").DefaultIfEmpty()
            from userRole in _dbConnection.UserRoles
                .Where(userRole => userRole.RoleId == role.Id && userRole.UserId == user.Id)
                .DefaultIfEmpty()
            select new UserProfile
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,
                IsAdmin = userRole != null,
                PersonId = user.PersonId,
                ResetPassword = user.ResetPassword
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