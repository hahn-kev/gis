using System;
using System.Threading.Tasks;
using Backend;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Npgsql;
using SendGrid.Helpers.Mail;
using Attachment = Backend.Entities.Attachment;
using IdentityUser = Backend.Entities.IdentityUser;

namespace UnitTestProject
{
    public class TestServerStartup : Startup
    {
        public TestServerStartup(IConfiguration configuration) : base(configuration)
        {
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
                loggingBuilder.SetMinimumLevel(LogLevel.Trace)
                    .AddConsole(options => options.IncludeScopes = true)
                    .AddDebug());
            base.ConfigureServices(services);
            services.Replace(ServiceDescriptor.Singleton<IEmailService>(provider =>
            {
                var esm = new Mock<EmailService>(provider.GetService<IOptions<Settings>>(),
                    provider.GetService<IOptions<TemplateSettings>>(),
                    provider.GetService<ILogger<EmailService>>());
                esm.CallBase = true;
                esm.Setup(email => email.SendEmail(It.IsAny<SendGridMessage>())).Returns(Task.CompletedTask);
                return esm.Object;
            }));
            services.Replace(ServiceDescriptor.Singleton<IEntityService>(provider =>
            {
                var esm = new Mock<EntityService>(provider.GetService<IDbConnection>());
//                esm.CallBase = true;
                return esm.Object;
            }));
        }

        public override void ConfigureDatabase(IServiceProvider provider)
        {
            DataConnection.AddDataProvider(nameof(MyDataProvider), new MyDataProvider());
            DataConnection.DefaultSettings = new MockDbSettings();
            var dbConnection = provider.GetService<IDbConnection>();
            TryCreateTable<IdentityUser>(dbConnection);
            TryCreateTable<IdentityUserClaim<int>>(dbConnection);
            TryCreateTable<IdentityUserLogin<int>>(dbConnection);
            TryCreateTable<IdentityUserToken<int>>(dbConnection);
            TryCreateTable<IdentityUserRole<int>>(dbConnection);
            TryCreateTable<IdentityRole<int>>(dbConnection);
            TryCreateTable<IdentityRoleClaim<int>>(dbConnection);
            TryCreateTable<PersonExtended>(dbConnection);
            TryCreateTable<PersonRole>(dbConnection);
            TryCreateTable<Job>(dbConnection);
            TryCreateTable<Grade>(dbConnection);
            TryCreateTable<Endorsement>(dbConnection);
            TryCreateTable<StaffEndorsement>(dbConnection);
            TryCreateTable<RequiredEndorsement>(dbConnection);
            TryCreateTable<Education>(dbConnection);
            TryCreateTable<OrgGroup>(dbConnection);
            TryCreateTable<LeaveRequest>(dbConnection);
            TryCreateTable<TrainingRequirement>(dbConnection);
            TryCreateTable<Staff>(dbConnection);
            TryCreateTable<StaffTraining>(dbConnection);
            TryCreateTable<EmergencyContact>(dbConnection);
            TryCreateTable<Donor>(dbConnection);
            TryCreateTable<Donation>(dbConnection);
            TryCreateTable<Evaluation>(dbConnection);
            TryCreateTable<Attachment>(dbConnection);
            TryCreateTable<MissionOrg>(dbConnection);
            TryCreateTable<MissionOrgYearSummary>(dbConnection);

            dbConnection.MappingSchema.SetConvertExpression<string, string[]>(
                s => s.Split(',', StringSplitOptions.RemoveEmptyEntries),
                true);
            dbConnection.MappingSchema.SetConvertExpression<string[], string>(s => string.Join(',', s));
        }

        private void TryCreateTable<T>(IDbConnection dbConnection)
        {
            try
            {
                dbConnection.CreateTable<T>();
            }
            catch (SqliteException e) when (e.SqliteErrorCode == 1) //already exists code I think
            {
            }
        }
    }
}