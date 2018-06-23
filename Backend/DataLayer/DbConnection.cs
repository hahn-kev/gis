using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Entities;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.Identity;
using LinqToDB.Mapping;
using Microsoft.AspNetCore.Identity;
using Npgsql;
using IdentityUser = Backend.Entities.IdentityUser;

namespace Backend.DataLayer
{
    public interface IDbConnection : IDataContext
    {
//        IQueryable<ImageInfo> Images { get; }
        IQueryable<Attachment> Attachments { get; }
        IQueryable<Person> People { get; }
        IQueryable<PersonExtended> PeopleExtended { get; }
        IQueryable<EmergencyContact> EmergencyContacts { get; }
        IQueryable<Donor> Donors { get; }
        IQueryable<Donation> Donations { get; }
        IQueryable<OrgGroup> OrgGroups { get; }
        IQueryable<MissionOrg> MissionOrgs { get; }
        IQueryable<MissionOrgYearSummary> MissionOrgYearSummaries { get; }
        IQueryable<PersonRole> PersonRoles { get; }
        IQueryable<Job> Job { get; }
        IQueryable<Grade> JobGrades { get; }
        IQueryable<Evaluation> Evaluations { get; }
        IQueryable<Endorsement> Endorsements { get; }
        IQueryable<StaffEndorsement> StaffEndorsements { get; }
        IQueryable<RequiredEndorsement> RequiredEndorsements { get; }
        IQueryable<LeaveRequest> LeaveRequests { get; }
        IQueryable<TrainingRequirement> TrainingRequirements { get; }
        IQueryable<Staff> Staff { get; }
        IQueryable<StaffTraining> StaffTraining { get; }
        IQueryable<IdentityUser> Users { get; }
        IQueryable<LinqToDB.Identity.IdentityUserClaim<int>> UserClaims { get; }
        IQueryable<LinqToDB.Identity.IdentityUserLogin<int>> UserLogins { get; }
        IQueryable<LinqToDB.Identity.IdentityUserRole<int>> UserRoles { get; }
        IQueryable<LinqToDB.Identity.IdentityUserToken<int>> UserTokens { get; }
        IQueryable<LinqToDB.Identity.IdentityRole<int>> Roles { get; }
        IQueryable<LinqToDB.Identity.IdentityRoleClaim<int>> RoleClaims { get; }
        void TryCreateTable<T>();
        DataConnectionTransaction BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        BulkCopyRowsCopied BulkCopy<T>(IEnumerable<T> list);
        System.Data.IDbConnection Connection { get; }
    }

    public class DbConnection : IdentityDataConnection<IdentityUser, LinqToDB.Identity.IdentityRole<int>, int>,
        IDbConnection
    {
        public DbConnection()
        {
            SetupMappingBuilder(MappingSchema);
        }

        private static bool _hasSetupMapping;

        public static void SetupMappingBuilder(MappingSchema mappingSchema)
        {
            if (_hasSetupMapping) return;
            var builder = mappingSchema.GetFluentMappingBuilder();
            builder.Entity<IdentityUser>().HasIdentity(user => user.Id);
            builder.Entity<LinqToDB.Identity.IdentityUserClaim<int>>().HasTableName("UserClaim")
                .HasIdentity(claim => claim.Id).HasPrimaryKey(claim => claim.Id);
            builder.Entity<LinqToDB.Identity.IdentityRole<int>>().HasTableName("Role").HasIdentity(role => role.Id);
            builder.Entity<LinqToDB.Identity.IdentityRoleClaim<int>>().HasTableName("RoleClaim")
                .HasIdentity(claim => claim.Id).HasPrimaryKey(claim => claim.Id);
            builder.Entity<LinqToDB.Identity.IdentityUserLogin<int>>().HasTableName("UserLogin");
            builder.Entity<LinqToDB.Identity.IdentityUserToken<int>>().HasTableName("UserToken");
            builder.Entity<LinqToDB.Identity.IdentityUserRole<int>>().HasTableName("UserRole");
            _hasSetupMapping = true;
        }


        public void TryCreateTable<T>()
        {
            try
            {
                this.CreateTable<T>();
            }
            catch (PostgresException e) when (e.SqlState == "42P07") //already exists code I think
            {
            }
        }

        IQueryable<IdentityUser> IDbConnection.Users => Users;
        IQueryable<LinqToDB.Identity.IdentityUserClaim<int>> IDbConnection.UserClaims => UserClaims;
        IQueryable<LinqToDB.Identity.IdentityUserLogin<int>> IDbConnection.UserLogins => UserLogins;
        IQueryable<LinqToDB.Identity.IdentityUserRole<int>> IDbConnection.UserRoles => UserRoles;
        IQueryable<LinqToDB.Identity.IdentityUserToken<int>> IDbConnection.UserTokens => UserTokens;
        IQueryable<LinqToDB.Identity.IdentityRole<int>> IDbConnection.Roles => Roles;
        IQueryable<LinqToDB.Identity.IdentityRoleClaim<int>> IDbConnection.RoleClaims => RoleClaims;

        public IQueryable<ImageInfo> Images => GetTable<ImageInfo>();
        public IQueryable<Attachment> Attachments => GetTable<Attachment>();
        public IQueryable<Person> People => GetTable<Person>();
        public IQueryable<PersonExtended> PeopleExtended => GetTable<PersonExtended>();
        public IQueryable<EmergencyContact> EmergencyContacts => GetTable<EmergencyContact>();
        public IQueryable<Donor> Donors => GetTable<Donor>();
        public IQueryable<Donation> Donations => GetTable<Donation>();
        public IQueryable<OrgGroup> OrgGroups => GetTable<OrgGroup>();
        public IQueryable<MissionOrg> MissionOrgs => GetTable<MissionOrg>();
        public IQueryable<MissionOrgYearSummary> MissionOrgYearSummaries => GetTable<MissionOrgYearSummary>();
        public IQueryable<PersonRole> PersonRoles => GetTable<PersonRole>();
        public IQueryable<Job> Job => GetTable<Job>();
        public IQueryable<Grade> JobGrades => GetTable<Grade>();
        public IQueryable<Evaluation> Evaluations => GetTable<Evaluation>();
        public IQueryable<Endorsement> Endorsements => GetTable<Endorsement>();
        public IQueryable<StaffEndorsement> StaffEndorsements => GetTable<StaffEndorsement>();
        public IQueryable<RequiredEndorsement> RequiredEndorsements => GetTable<RequiredEndorsement>();
        public IQueryable<LeaveRequest> LeaveRequests => GetTable<LeaveRequest>();
        public IQueryable<TrainingRequirement> TrainingRequirements => GetTable<TrainingRequirement>();
        public IQueryable<Staff> Staff => GetTable<Staff>();
        public IQueryable<StaffTraining> StaffTraining => GetTable<StaffTraining>();

        public BulkCopyRowsCopied BulkCopy<T>(IEnumerable<T> list)
        {
            return DataConnectionExtensions.BulkCopy(this, list);
        }
    }
}