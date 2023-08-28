using Backend;
using Backend.Controllers;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("client_id.json");
builder.WebHost.UseSentry(options =>
{
    options.Release = typeof(JWTSettings).Assembly.GetName().Version?.ToString();
    options.SetBeforeSend(sentryEvent =>
    {
        switch (sentryEvent.Exception)
        {
            case UserError ue: return null;
        }

        if (sentryEvent.Environment?.Equals("production", StringComparison.OrdinalIgnoreCase) == false)
            return null;

        return sentryEvent;
    });
});

builder.Logging.AddFile("Logs/log-{Date}.txt", LogLevel.Warning);
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);
var app = builder.Build();
startup.Configure(app, app.Environment);

app.Run();