using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using Backend.Utils;
using LinqToDB.Data;
using LinqToDB.Identity;
using LinqToDB.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Npgsql;
using Sentinel.Sdk.Extensions;
using Sentinel.Sdk.Middleware;
using IdentityUser = Backend.Entities.IdentityUser;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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
            var settings = Configuration.Get<Settings>();
            services.Configure<JWTSettings>(Configuration.GetSection("JWTSettings"));
            services.Configure<TemplateSettings>(Configuration.GetSection("TemplateSettings"));
            AddIdentity<IdentityUser, LinqToDB.Identity.IdentityRole<int>>(services,
                    options =>
                    {
                        options.SignIn.RequireConfirmedEmail = false;
                        options.SignIn.RequireConfirmedPhoneNumber = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequireDigit = false;
                        options.Password.RequireLowercase = false;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequiredLength = 8;
                    })
                .AddLinqToDBStores<int>(new DefaultConnectionFactory())
                .AddDefaultTokenProviders();

            services.AddSentinel(new SentinelSettings
            {
                Dsn = settings.SentryDsn,
                Environment = (settings.Environment ?? "production").ToLower(),
                IncludeRequestData = true,
                IncludeCookies = false,
                Release = GetType().Assembly.GetName().Version.ToString(),
                ServerName = settings.BaseUrl,
                IgnoreTypes = new List<Type> {typeof(UserError)}
            });

            services.AddMvc(options =>
                {
                    options.InputFormatters.Add(new TextPlainInputFormatter());
#if DEBUG
                    options.Filters.Add(typeof(AllowAnonymousFilter));
#else
//require auth on every controller by default
                    options.Filters.Add(
                        new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
#endif
                    options.Filters.Add(typeof(GlobalExceptionHandler));
                })
                .AddJsonOptions(options =>
                {
                    //time zone info won't be included, this is so we can pass a date from the front end without the timezone
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
                });
            services.AddResponseCaching();
            services.AddResponseCompression(options =>
            {
                options.Providers.Add(new BrotliCompressionProvider());
                options.EnableForHttps = true;
            });
//            services.AddLocalization();

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
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

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
                })
                .AddGoogle(options =>
                {
                    var google = Configuration.GetSection("web");
                    options.ClientId = google["client_id"];
                    options.ClientSecret = google["client_secret"];
                    options.Scope.Add("https://www.googleapis.com/auth/drive");
                    options.SaveTokens = true;
                    options.Events.OnTicketReceived = async context =>
                    {
                        context.HandleResponse();
                        context.HttpContext.User = context.Principal;

                        var authenticateController =
                            context.HttpContext.RequestServices.GetRequiredService<AuthenticateController>();
                        authenticateController.ControllerContext.HttpContext = context.HttpContext;
                        await authenticateController.GoogleSignIn(context.Properties);
                        context.Response.Redirect(context.ReturnUri);
                    };
                });
            //todo addGoogle for authentication
            services.AddAuthorization(options =>
            {
                options.AddPolicy("attachments", builder => builder.RequireRole("admin", "hr"));
                options.AddPolicy("jobs", builder => builder.RequireRole("admin", "hr"));
                options.AddPolicy("grades", builder => builder.RequireRole("admin", "hr"));
                options.AddPolicy("role", builder => builder.RequireRole("admin", "hr"));
                options.AddPolicy("evaluations", builder => builder.RequireRole("admin", "hr"));
                options.AddPolicy("staff", builder => builder.RequireRole("admin", "hr"));
                options.AddPolicy("contact", builder => builder.RequireRole("admin", "hr"));
                options.AddPolicy("isSupervisor",
                    builder => builder.RequireClaim(AuthenticateController.ClaimSupervisor));
                options.AddPolicy("leaveRequest", builder => builder.RequireRole("admin", "hr"));
                options.AddPolicy("training", builder => builder.RequireRole("admin", "hr"));
                options.AddPolicy("orgGroup", builder => builder.RequireRole("admin", "hr"));
                options.AddPolicy("people", builder => builder.RequireRole("admin", "hr", "registrar"));
                options.AddPolicy("sendingOrg", builder => builder.RequireRole("admin", "hr", "registrar"));
                options.AddPolicy("orgTreeData",
                    builder => builder.RequireAssertion(context =>
                        context.User.IsSupervisor() || context.User.IsAdminOrHr()));
            });
            foreach (var type in GetType().Assembly.GetTypes()
                .Where(type =>
                    (type.Name.Contains("Service") || type.Name.Contains("Repository")) && !type.IsInterface))
            {
                var interfaces = type.GetInterfaces();
                if (interfaces.Length > 0)
                {
                    foreach (var @interface in interfaces)
                    {
                        services.AddScoped(@interface, type);
                    }
                }
                else
                {
                    services.AddScoped(type);
                }
            }

            services.AddScoped<AuthenticateController>();
            services.AddScoped<IDbConnection, DbConnection>();
            services.AddScoped(provider =>
                new NpgsqlLargeObjectManager(
                    (NpgsqlConnection) provider.GetRequiredService<IDbConnection>().Connection));
        }

        private IdentityBuilder AddIdentity<TUser, TRole>(IServiceCollection services,
            Action<IdentityOptions> setupAction) where TUser : class where TRole : class
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
            return new IdentityBuilder(typeof(TUser), typeof(TRole), services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddFile("Logs/log-{Date}.txt", LogLevel.Warning);
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
                //redirect to login
                if (context.Response.StatusCode == 401 && !context.IsJsonRequest())
                {
                    context.Response.Redirect(ControllerExtensions.RedirectLogin(context.Request.GetDisplayUrl()));
                }

                if (context.Response.StatusCode == 404 &&
                    !Path.HasExtension(context.Request.Path.Value) &&
                    !context.Request.Path.Value.StartsWith("/api/"))
                {
//                    var requestCulture = context.Features.Get<IRequestCultureFeature>().RequestCulture;
//                    context.Request.Path = $"/{requestCulture.Culture.TwoLetterISOLanguageName}/index.html";
                    context.Request.Path = "/index.html";
                    await next();
                }
            });
            app.UseResponseCaching();
            app.UseResponseCompression();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context => context.Context.Response.GetTypedHeaders().CacheControl =
                    new CacheControlHeaderValue
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromSeconds(30)
                    }
            });
            app.UseAuthentication();
            app.UseSentinel();
            app.UseMvc();

            ConfigureDatabase(app.ApplicationServices, loggerFactory.CreateLogger("database"));
#if DEBUG
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dbConnection = scope.ServiceProvider.GetService<IDbConnection>();
                var roleManager = scope.ServiceProvider.GetService<RoleManager<LinqToDB.Identity.IdentityRole<int>>>();
                var missingRoles =
                    new[] {"admin", "hr", "hradmin", "registrar"}.Except(roleManager.Roles.Select(role => role.Name));
                foreach (var missingRole in missingRoles)
                {
                    roleManager.CreateAsync(new LinqToDB.Identity.IdentityRole<int>(missingRole)).Wait();
                }

                dbConnection.TryCreateTable<Donor>();
                //to configure db look at ServiceFixture.SetupSchema
                if (!dbConnection.Users.Any())
                {
                    var userService = scope.ServiceProvider.GetService<UserService>();
                    var identityUser = new IdentityUser
                    {
                        UserName = "khahn",
                        ResetPassword = true
                    };
                    userService.CreateAsync(identityUser, "password").Wait();
                    userService.AddToRoleAsync(identityUser, "admin").Wait();
                }
            }
#endif
        }

        public void ConfigureDatabase(IServiceProvider provider, ILogger logger)
        {
            var settings = provider.GetService<IOptions<Settings>>().Value;
            DataConnection.DefaultSettings = settings;
            DataConnection.TurnTraceSwitchOn();
            DataConnection.WriteTraceLine = (message, category) => logger.LogDebug(message);
            LinqToDB.Common.Configuration.Linq.AllowMultipleQuery = true;
            DbConnection.SetupMappingBuilder(MappingSchema.Default);
        }
    }
}