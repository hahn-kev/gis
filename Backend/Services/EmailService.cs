using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RestEase;

namespace Backend.Services
{
    public class EmailService
    {
        private static readonly IMailGunApi MailGunApi = RestClient.For<IMailGunApi>("https://api.mailgun.net/v3");
        private readonly string _domain;

        public EmailService(IOptions<Settings> options)
        {
            var apiKey = options.Value.MailgunApiKey;
            if (string.IsNullOrEmpty(apiKey)) throw new NullReferenceException("MailgunApiKey setting can not be null");
            var value = Convert.ToBase64String(Encoding.ASCII.GetBytes("api:" + apiKey));
            MailGunApi.Authorization = new AuthenticationHeaderValue("Basic", value);
            _domain = options.Value.MailgunDomain;
            if (string.IsNullOrEmpty(_domain))
                throw new NullReferenceException("MailgunDomain setting can not be null");
        }

        public async Task<MailgunReponse> SendEmail(string to, string subject, string body)
        {
            var mailgunReponse =
                await MailGunApi.SendEmail(_domain, "GIS GIS@" + _domain, to, subject, body);
            return mailgunReponse;
        }
    }

    public interface IMailGunApi
    {
        [Header("Authorization")]
        AuthenticationHeaderValue Authorization { get; set; }

        [Post("{domain}/messages")]
        Task<MailgunReponse> SendEmail([Path] string domain, string from, string to, string subject, string text);
    }

    public class MailgunReponse
    {
        public string Message;
        public string Id;
    }
}