﻿using System.Linq;
using System.Threading.Tasks;
using Backend.Entities;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Identity;
using Microsoft.AspNetCore.Identity;
using Npgsql;
using IdentityUser = Backend.Entities.IdentityUser;
    
namespace Backend.DataLayer
{
    public class DbConnection : IdentityDataConnection<IdentityUser, LinqToDB.Identity.IdentityRole<int>, int>
    {
        private readonly RoleManager<LinqToDB.Identity.IdentityRole<int>> _roleManager;

        public DbConnection(RoleManager<LinqToDB.Identity.IdentityRole<int>> roleManager)
        {
            _roleManager = roleManager;

            var builder = MappingSchema.GetFluentMappingBuilder();
            builder.Entity<IdentityUser>().HasIdentity(user => user.Id);
            builder.Entity<LinqToDB.Identity.IdentityUserClaim<int>>().HasTableName("UserClaim").HasIdentity(claim => claim.Id);
            builder.Entity<LinqToDB.Identity.IdentityRole<int>>().HasTableName("Role").HasIdentity(role => role.Id);
            builder.Entity<LinqToDB.Identity.IdentityRoleClaim<int>>().HasTableName("RoleClaim").HasIdentity(claim => claim.Id);
            builder.Entity<LinqToDB.Identity.IdentityUserLogin<int>>().HasTableName("UserLogin");
            builder.Entity<LinqToDB.Identity.IdentityUserToken<int>>().HasTableName("UserToken");
            builder.Entity<LinqToDB.Identity.IdentityUserRole<int>>().HasTableName("UserRole");
        }

        public ITable<ImageInfo> Images => GetTable<ImageInfo>();
        public ITable<Person> People => GetTable<Person>();
        public ITable<PersonExtended> PeopleExtended => GetTable<PersonExtended>();
        public ITable<PersonRole> PersonRoles => GetTable<PersonRole>();

        public IQueryable<UserProfile> UserProfiles
        {
            get
            {
                return from user in GetTable<UserProfile>()
                    from role in Roles.Where(role => role.Name == "admin").DefaultIfEmpty()
                    from userRole in UserRoles
                        .Where(userRole => userRole.RoleId == role.Id && userRole.UserId == user.Id)
                        .DefaultIfEmpty()
                    select new UserProfile
                    {
                        Id = user.Id,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        UserName = user.UserName,
                        IsAdmin = userRole != null
                    };
            }
        }

        public async Task Setup()
        {
#if DEBUG
            TryCreateTable<IdentityUser>();
            TryCreateTable<LinqToDB.Identity.IdentityUserClaim<int>>();
            TryCreateTable<LinqToDB.Identity.IdentityUserLogin<int>>();
            TryCreateTable<LinqToDB.Identity.IdentityUserToken<int>>();
            TryCreateTable<LinqToDB.Identity.IdentityUserRole<int>>();
            TryCreateTable<LinqToDB.Identity.IdentityRole<int>>();
            TryCreateTable<LinqToDB.Identity.IdentityRoleClaim<int>>();
            TryCreateTable<ImageInfo>();
            TryCreateTable<PersonExtended>();
            TryCreateTable<PersonRole>();
            var roles = new[] {"admin"};
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new LinqToDB.Identity.IdentityRole<int>(role));
                }
            }
#endif
        }

        private void TryCreateTable<T>()
        {
            try
            {
                this.CreateTable<T>();
            }
            catch (PostgresException e) when (e.SqlState == "42P07") //already exists code I think
            {
            }
        }
    }
}