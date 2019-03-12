using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Entities;
using Backend.Linq2DbIdentity;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider;
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
        IQueryable<Education> Education { get; }
        IQueryable<LeaveRequest> LeaveRequests { get; }
        IQueryable<Holiday> Holidays { get; }
        IQueryable<TrainingRequirement> TrainingRequirements { get; }
        IQueryable<Staff> Staff { get; }
        IQueryable<StaffTraining> StaffTraining { get; }
        IQueryable<IdentityUser> Users { get; }
        IQueryable<IdentityUserClaim<int>> UserClaims { get; }
        IQueryable<IdentityUserLogin<int>> UserLogins { get; }
        IQueryable<IdentityUserRole<int>> UserRoles { get; }
        IQueryable<IdentityUserToken<int>> UserTokens { get; }
        IQueryable<IdentityRole<int>> Roles { get; }
        IQueryable<IdentityRoleClaim<int>> RoleClaims { get; }
        void TryCreateTable<T>();
        DataConnectionTransaction BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        BulkCopyRowsCopied BulkCopy<T>(IEnumerable<T> list) where T : class;
        System.Data.IDbConnection Connection { get; }
        IDataProvider DataProvider { get; }
    }

    public class DbConnection :
        IdentityDataConnection<IdentityUser, IdentityRole<int>, int>,
        IDbConnection,
        IDisposable
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
            builder.Entity<IdentityUserClaim<int>>().HasTableName("UserClaim")
                .HasIdentity(claim => claim.Id).HasPrimaryKey(claim => claim.Id);
            builder.Entity<IdentityRole<int>>().HasTableName("Role").HasIdentity(role => role.Id);
            builder.Entity<IdentityRoleClaim<int>>().HasTableName("RoleClaim")
                .HasIdentity(claim => claim.Id).HasPrimaryKey(claim => claim.Id);
            builder.Entity<IdentityUserLogin<int>>().HasTableName("UserLogin");
            builder.Entity<IdentityUserToken<int>>().HasTableName("UserToken");
            builder.Entity<IdentityUserRole<int>>().HasTableName("UserRole");
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
        IQueryable<IdentityUserClaim<int>> IDbConnection.UserClaims => UserClaims;
        IQueryable<IdentityUserLogin<int>> IDbConnection.UserLogins => UserLogins;
        IQueryable<IdentityUserRole<int>> IDbConnection.UserRoles => UserRoles;
        IQueryable<IdentityUserToken<int>> IDbConnection.UserTokens => UserTokens;
        IQueryable<IdentityRole<int>> IDbConnection.Roles => Roles;
        IQueryable<IdentityRoleClaim<int>> IDbConnection.RoleClaims => RoleClaims;

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
        public IQueryable<Education> Education => GetTable<Education>();
        public IQueryable<LeaveRequest> LeaveRequests => GetTable<LeaveRequest>();
        public IQueryable<Holiday> Holidays => GetTable<Holiday>();
        public IQueryable<TrainingRequirement> TrainingRequirements => GetTable<TrainingRequirement>();
        public IQueryable<Staff> Staff => GetTable<Staff>();
        public IQueryable<StaffTraining> StaffTraining => GetTable<StaffTraining>();

        public BulkCopyRowsCopied BulkCopy<T>(IEnumerable<T> list) where T : class
        {
            return DataConnectionExtensions.BulkCopy(this, list);
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }
    }

    public class AppConnectionFactory : IConnectionFactory
    {
        private readonly IDbConnection _dbConnection;

        public AppConnectionFactory(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public IDataContext GetContext()
        {
            return GetConnection();
        }

        public DataConnection GetConnection()
        {
            //we're creating a new dataConnection here because the Identity stores dispose
            //the connection they get from here, and by injecting in this way the connection won't get disposed
            //which would break unit tests
            return new DataConnection(_dbConnection.DataProvider, _dbConnection.Connection);
        }
    }
}