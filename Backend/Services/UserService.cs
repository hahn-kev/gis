using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.DataLayer;
using Backend.Entities;
using Microsoft.AspNetCore.Identity;
using IdentityUser = Backend.Entities.IdentityUser;

namespace Backend.Services
{
    public class UserService
    {
        private readonly UsersRepository _usersRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly PersonRepository _personRepository;

        public UserService(UsersRepository usersRepository,
            UserManager<IdentityUser> userManager,
            PersonRepository personRepository)
        {
            _usersRepository = usersRepository;
            _userManager = userManager;
            _personRepository = personRepository;
        }

        public IQueryable<UserProfile> Users => _usersRepository.Users;

        public UserProfile UserByName(string name)
        {
            return _usersRepository.UserByName(name);
        }

        public void UpdatePersonId(int id, Guid personId)
        {
            _usersRepository.UpdatePersonId(id, personId);
        }

        public UserProfile UserById(int id)
        {
            return _usersRepository.UserById(id);
        }

        public void DeleteUser(int id)
        {
            _usersRepository.DeleteUser(id);
        }

        private void CheckUpdatePersonId(IUser user)
        {
            if (string.IsNullOrEmpty(user.Email) || user.PersonId.HasValue) return;
            user.PersonId = _personRepository.People
                .Where(person => person.Email == user.Email)
                .Select(person => person.Id)
                .SingleOrDefault();
        }

        public Task<IdentityResult> CreateAsync(IdentityUser user)
        {
            CheckUpdatePersonId(user);
            return _userManager.CreateAsync(user);
        }

        public Task<IdentityResult> UpdateAsync(IdentityUser user)
        {
            CheckUpdatePersonId(user);
            return _userManager.UpdateAsync(user);
        }

        public Task<IdentityResult> CreateAsync(IdentityUser user, string password)
        {
            return _userManager.CreateAsync(user, password);
        }

        public Task<IdentityResult> ChangePasswordAsync(IdentityUser user, string currentPassword, string newPassword)
        {
            return _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public Task<IdentityResult> AddLoginAsync(IdentityUser user, UserLoginInfo login)
        {
            return _userManager.AddLoginAsync(user, login);
        }

        public Task<IdentityUser> FindByLoginAsync(string loginProvider, string providerKey)
        {
            return _userManager.FindByLoginAsync(loginProvider, providerKey);
        }

        public Task<IdentityUser> FindByEmailAsync(string email)
        {
            return _userManager.FindByEmailAsync(email);
        }

        public Task<IdentityUser> FindByNameAsync(string userName)
        {
            return _userManager.FindByNameAsync(userName);
        }
    }
}