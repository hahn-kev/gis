using System;
using Backend.Controllers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => builder.AddJsonFile("client_id.json"))
                .UseSentry(options =>
                {
                    options.Release = typeof(Program).Assembly.GetName().Version.ToString();
                    options.BeforeSend = sentryEvent =>
                    {
                        switch (sentryEvent.Exception)
                        {
                            case UserError ue: return null;
                        }

                        if (sentryEvent.Environment?.Equals("production", StringComparison.OrdinalIgnoreCase) == false)
                            return null;

                        return sentryEvent;
                    };
                })
                .UseStartup<Startup>();
    }
}