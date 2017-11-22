using System.Linq;
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
    public class DbConnection : IdentityDataConnection<IdentityUser, IdentityRole<int>, int>
    {
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public DbConnection(RoleManager<IdentityRole<int>> roleManager)
        {
            _roleManager = roleManager;

            var builder = MappingSchema.GetFluentMappingBuilder();
            builder.Entity<IdentityUser>().HasIdentity(user => user.Id);
            builder.Entity<IdentityUserClaim<int>>().HasTableName("UserClaim").HasIdentity(claim => claim.Id);
            builder.Entity<IdentityRole<int>>().HasTableName("Role").HasIdentity(role => role.Id);
            builder.Entity<IdentityRoleClaim<int>>().HasTableName("RoleClaim").HasIdentity(claim => claim.Id);
            builder.Entity<IdentityUserLogin<int>>().HasTableName("UserLogin");
            builder.Entity<IdentityUserToken<int>>().HasTableName("UserToken");
            builder.Entity<IdentityUserRole<int>>().HasTableName("UserRole");
        }

        public ITable<ImageInfo> Images => GetTable<ImageInfo>();

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
            TryCreateTable<IdentityUserClaim<int>>();
            TryCreateTable<IdentityUserLogin<int>>();
            TryCreateTable<IdentityUserToken<int>>();
            TryCreateTable<IdentityUserRole<int>>();
            TryCreateTable<IdentityRole<int>>();
            TryCreateTable<IdentityRoleClaim<int>>();
            TryCreateTable<ImageInfo>();

            var roles = new[] {"admin"};
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole<int>(role));
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