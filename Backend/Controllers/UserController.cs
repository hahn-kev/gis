using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using IdentityUser = Backend.Entities.IdentityUser;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class UserController : MyController
    {
        private readonly UserService _userService;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(UserManager<IdentityUser> userManager, UserService userService)
        {
            _userManager = userManager;
            _userService = userService;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public IReadOnlyCollection<IUser> Users()
        {
            return _userService.Users.ToList();
        }

        [Authorize(Roles = "admin")]
        [HttpGet("{name}")]
        public UserProfile Get(string name)
        {
            return _userService.UserByName(name);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public Task<IActionResult> Put(int id, [FromBody] RegisterUser registerUser)
        {
            return UpdateUser(id, registerUser);
        }

        [HttpGet("self")]
        public UserProfile Self()
        {
            return _userService.UserById(int.Parse(_userManager.GetUserId(User)));
        }

        [HttpPut("self")]
        public Task<IActionResult> Self([FromBody] RegisterUser registerUser)
        {
            var userId = int.Parse(_userManager.GetUserId(User));
            if (userId != registerUser.Id)
            {
                throw new AuthenticationException("preventing user from changing another users details");
            }
            if (registerUser.UserName != _userManager.GetUserName(User))
            {
                throw new AuthenticationException("User isn't allowed to change their own username");
            }
            return UpdateUser(userId, registerUser);
        }

        [HttpPost("password")]
        public async Task<IActionResult> Password(string newPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, newPassword);
            return !result.Succeeded ? throw result.Errors() : Ok();
        }

        private async Task<IActionResult> UpdateUser(int id, RegisterUser registerUser)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            user.CopyFrom(registerUser);
            await _userService.UpdateAsync(user);
            if (string.IsNullOrEmpty(registerUser.Password)) return Ok();

            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, registerUser.Password);
            return !result.Succeeded ? throw result.Errors() : Ok();
        }

        [Authorize(Roles = "admin")]
        [HttpPut("grant/{role}/{id}")]
        public async Task<IActionResult> GrantRole(string role, int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) throw new NullReferenceException("User not found");
            var identityResult = await _userManager.AddToRoleAsync(user, role);
            if (!identityResult.Succeeded) throw identityResult.Errors();
            return Ok();
        }

        [Authorize(Roles = "admin")]
        [HttpPut("revoke/{role}/{id}")]
        public async Task<IActionResult> RevokeRole(string role, int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) throw new NullReferenceException("User not found");
            var identityResult = await _userManager.RemoveFromRoleAsync(user, role);
            if (!identityResult.Succeeded) throw identityResult.Errors();
            return Ok();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            _userService.DeleteUser(id);
            return Ok();
        }
    }
}