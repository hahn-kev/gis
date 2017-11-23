using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.DataLayer;
using Backend.Services;
using LinqToDB.Data;
using LinqToDB.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Sentinel.Sdk.Extensions;
using Sentinel.Sdk.Middleware;
using IdentityUser = Backend.Entities.IdentityUser;

namespace Backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<Settings>(Configuration);
            services.Configure<JWTSettings>(Configuration.GetSection("JWTSettings"));
            AddIdentity<IdentityUser, IdentityRole<int>>(services, options =>
                {
                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 8;
                })
                .AddLinqToDBStores<int>(new DefaultConnectionFactory());
            services.AddSentinel(new SentinelSettings
            {
                Dsn = Configuration.GetValue<string>("SentryDsn"),
                Environment = Configuration.GetValue<string>("Environment") ?? "Production",
                IncludeRequestData = true,
                ServerName = Configuration.GetValue<string>("BaseUrl")
            });
            
            services.AddMvc(options =>
            {
                options.InputFormatters.Add(new TextPlainInputFormatter());
                //require auth on every controller by default
                options.Filters.Add(
                    new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
                options.Filters.Add(typeof(GlobalExceptionHandler));
            });
            //todo response caching?
//            services.AddResponseCaching();
            services.AddLocalization();

            //todo localization?
//            services.Configure<RequestLocalizationOptions>(options =>
//            {
//                var fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
//                var localeFolders = fileProvider.GetDirectoryContents("wwwroot")
//                    .Where(info => info.IsDirectory && info.Name != "en").Select(info => info.Name);
//                foreach (var localeFolder in localeFolders)
//                {
//                    options.SupportedCultures.Add(new CultureInfo(localeFolder));
//                }
//            });
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
            {
                var jwtSettings = Configuration.GetSection("JWTSettings").Get<JWTSettings>();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey)),

                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies[AuthenticateController.JwtCookieName];
                        return Task.CompletedTask;
                    }
                };
            });
            //todo addGoogle for authentication
//            services.AddAuthorization();
            services.AddScoped<UsersRepository>();
            services.AddScoped<ImageRepository>();
            services.AddScoped<EmailService>();
            services.AddScoped<DbConnection>();
            services.AddScoped(provider =>
                new NpgsqlLargeObjectManager(
                    (NpgsqlConnection) provider.GetRequiredService<DbConnection>().Connection));
        }

        private IdentityBuilder AddIdentity<TUser, TRole>(IServiceCollection services, Action<IdentityOptions> setupAction) where TUser : class where TRole : class
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
            services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
            services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
            services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            services.TryAddScoped<IRoleValidator<TRole>, RoleValidator<TRole>>();
            services.TryAddScoped<IdentityErrorDescriber>();
            services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<TUser>>();
            services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser, TRole>>();
            services.TryAddScoped<UserManager<TUser>, AspNetUserManager<TUser>>();
            services.TryAddScoped<SignInManager<TUser>, SignInManager<TUser>>();
            services.TryAddScoped<RoleManager<TRole>, AspNetRoleManager<TRole>>();
            if (setupAction != null)
                services.Configure<IdentityOptions>(setupAction);
            return new IdentityBuilder(typeof (TUser), typeof (TRole), services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole().AddFile("Logs/log-{Date}.txt", LogLevel.Warning);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
//            app.UseRequestLocalization();
            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404 &&
                    !Path.HasExtension(context.Request.Path.Value) &&
                    !context.Request.Path.Value.StartsWith("/api/"))
                {
//                    var requestCulture = context.Features.Get<IRequestCultureFeature>().RequestCulture;
//                    context.Request.Path = $"/{requestCulture.Culture.TwoLetterISOLanguageName}/index.html";
                    context.Request.Path = "index.html";
                    await next();
                }
            });
            app.UseStaticFiles();
//            app.UseResponseCaching();
            app.UseAuthentication();
            app.UseSentinel();
            app.UseMvc();
            var settings = app.ApplicationServices.GetService<IOptions<Settings>>().Value;
            DataConnection.DefaultSettings = settings;
            LinqToDB.Common.Configuration.Linq.AllowMultipleQuery = true;

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var hereForYouConnection = scope.ServiceProvider.GetService<DbConnection>();
                if (env.IsDevelopment())
                {
                    DataConnection.TurnTraceSwitchOn();
                    DataConnection.WriteTraceLine = (message, category) => Debug.WriteLine(message, category);
                    hereForYouConnection.Setup().Wait();
                }
            }
        }
    }
}