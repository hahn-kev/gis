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

        public override void AddDatabase(IServiceCollection services)
        {
            services.AddSingleton<IDbConnection, DbConnection>();
            DataConnection.AddDataProvider(nameof(MyDataProvider), new MyDataProvider());
            DataConnection.DefaultSettings = new MockDbSettings();
        }
    }
}