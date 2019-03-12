using System.Threading.Tasks;
using Backend;
using Backend.DataLayer;
using Backend.Services;
using LinqToDB.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SendGrid.Helpers.Mail;

namespace UnitTestProject
{
    public class TestServerStartup : Startup
    {
        public TestServerStartup(IConfiguration configuration) : base(configuration)
        {
        }

        public override void ConfigureServices(IServiceCollection services)
        {
#if DEBUG
            services.AddLogging(loggingBuilder =>
                loggingBuilder.SetMinimumLevel(LogLevel.Trace)
                    .AddConsole(options => options.IncludeScopes = true)
                    .AddDebug());
#endif

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