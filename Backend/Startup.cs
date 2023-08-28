using System.Text;
using Backend.Authorization;
using Backend.Controllers;
using Backend.DataLayer;
using Backend.Linq2DbIdentity;
using Backend.Services;
using Backend.Utils;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Npgsql;
using IdentityUser = Backend.Entities.IdentityUser;

namespace Backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<Settings>(Configuration);
            var settings = Configuration.Get<Settings>();
            services.Configure<JWTSettings>(Configuration.GetSection("JWTSettings"));
            services.Configure<TemplateSettings>(Configuration.GetSection("TemplateSettings"));
            AddIdentity<IdentityUser, IdentityRole<int>>(services,
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
                .AddLinqToDBStores<int>(new AppConnectionFactory(null)) //passing an invalid factory here
                .AddDefaultTokenProviders();
            //replace the singleton factory above with a scoped version
            services.Replace(ServiceDescriptor.Scoped<IConnectionFactory, AppConnectionFactory>());

            services.AddMvc(options =>
                {
                    options.InputFormatters.Add(new TextPlainInputFormatter());

//require auth on every controller by default
                    options.Filters.Add(
                        new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));

                    options.Filters.Add(typeof(GlobalExceptionHandler));
                })
                .AddControllersAsServices()
                .AddNewtonsoftJson(options =>
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

                    //disable webviews, shouldn't casue an issue but we are getting a warning about it.
                    // https://developers.googleblog.com/2021/06/upcoming-security-changes-to-googles-oauth-2.0-authorization-endpoint.html#test
                    if (Configuration.GetValue<bool>("DisableGoogleWebView"))
                        options.AuthorizationEndpoint = QueryHelpers.AddQueryString(options.AuthorizationEndpoint,
                            "disallow_webview",
                            "true");

                    options.UserInformationEndpoint = "https://openidconnect.googleapis.com/v1/userinfo";
                    options.ClaimActions.Clear();
                    options.ClaimActions.MapJsonKey(
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                        "sub");
                    options.ClaimActions.MapJsonKey("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
                        "name");
                    options.ClaimActions.MapJsonKey("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname",
                        "given_name");
                    options.ClaimActions.MapJsonKey("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname",
                        "family_name");
                    options.ClaimActions.MapJsonKey("urn:google:profile", "profile");
                    options.ClaimActions.MapJsonKey(
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
                        "email");

                    options.ClientId = google["client_id"];
                    options.ClientSecret = google["client_secret"];
//                    options.Scope.Add("https://www.googleapis.com/auth/drive");
                    options.SaveTokens = true;
//                    options.AccessType = "offline";
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
            services.AddMyAuthorization();
            foreach (var type in typeof(Startup).Assembly.GetTypes()
                         .Where(type =>
                             (type.Name.Contains("Service") || type.Name.Contains("Repository")) && !type.IsInterface))
            {
                if (type == typeof(DbStartupService))
                    continue;
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

            services.AddHostedService<DbStartupService>();
            AddDatabase(services);
            services.AddScoped(provider =>
                new NpgsqlLargeObjectManager(
                    (NpgsqlConnection)provider.GetRequiredService<IDbConnection>().Connection));

#if DEBUG
            services.AddSwaggerDocument();
#endif
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
            services.Configure(setupAction);
            return new IdentityBuilder(typeof(TUser), typeof(TRole), services);
        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
            });
            app.UseResponseCaching();
            app.UseResponseCompression();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSentryTracing();
            app.UseAuthentication();

#if DEBUG
            app.UseSwagger();
            app.UseSwaggerUi3();
#endif
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }

        public virtual void AddDatabase(IServiceCollection services)
        {
            services.AddScoped<IDbConnection, DbConnection>();
            DataConnection.DefaultSettings = Configuration.Get<Settings>();
        }
    }
}