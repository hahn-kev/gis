using System;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class SettingsController : MyController
    {
        private readonly IConfigurationSection _googleOptions;
        private readonly Settings _settings;

        public SettingsController(IOptions<Settings> settings, IConfiguration config)
        {
            _googleOptions = config.GetSection("web");
            _settings = settings.Value;
        }

        [HttpGet]
        public IActionResult Settings()
        {
            return Json(new
            {
                version = GetType().Assembly.GetName().Version.ToString(),
                googleAPIKey = _settings.GoogleAPIKey,
                googleClientId = _googleOptions["client_id"]
            });
        }
    }
}