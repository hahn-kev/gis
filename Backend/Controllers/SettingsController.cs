using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class SettingsController : MyController
    {
        private readonly Settings _settings;

        public SettingsController(IOptions<Settings> settings)
        {
            _settings = settings.Value;
        }

        [HttpGet]
        public IActionResult Settings()
        {
            return Json(new
            {
                version = GetType().Assembly.GetName().Version.ToString()
//                _settings.DiscourseBaseUrl
            });
        }

        //todo localization?
//        [HttpGet("setLanguage/{culture}")]
//        public IActionResult SetLanguage(string culture)
//        {
//            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
//                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
//                new CookieOptions {Expires = DateTimeOffset.UtcNow.AddYears(1)});
//            return LocalRedirect("/" + culture);
//        }
    }
}