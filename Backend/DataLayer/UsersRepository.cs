using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Entities;
using LinqToDB;

namespace Backend.DataLayer
{
    public class UsersRepository
    {
        private readonly DbConnection _dbConnection;

        public UsersRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public IQueryable<UserProfile> Users => _dbConnection.UserProfiles;

        public UserProfile UserByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return Users.FirstOrDefault(user =>
                user.UserName == name);
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