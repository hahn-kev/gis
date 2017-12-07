using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Backend.Entities;
using Microsoft.Extensions.Options;
using RestEase;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Backend.Services
{
    public interface IEmailService
    {
        Task<MailgunReponse> SendEmail(string to, string subject, string body);

        Task SendTemplateEmail(Dictionary<string, string> substituions,
            string templateId,
            PersonExtended from,
            PersonExtended to);

        Task SendTemplateEmail(Dictionary<string, string> substituions,
            string templateId,
            string toEmail,
            string toName,
            string fromEmail,
            string fromName);
    }

    public class EmailService : IEmailService
    {
        private static readonly IMailGunApi MailGunApi = RestClient.For<IMailGunApi>("https://api.mailgun.net/v3");
        private readonly SendGridClient _sendGridClient;
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
            _sendGridClient = new SendGridClient(options.Value.SendGridAPIKey);
        }

        public async Task<MailgunReponse> SendEmail(string to, string subject, string body)
        {
            var mailgunReponse =
                await MailGunApi.SendEmail(_domain, "GIS GIS@" + _domain, to, subject, body);
            return mailgunReponse;
        }

        public Task SendTemplateEmail(Dictionary<string, string> substituions,
            string templateId,
            PersonExtended from,
            PersonExtended to)
        {
            return SendTemplateEmail(substituions,
                templateId,
                from.Email,
                from.PreferredName,
                to.Email,
                to.PreferredName);
        }

        public async Task SendTemplateEmail(Dictionary<string, string> substituions,
            string templateId,
            string toEmail,
            string toName,
            string fromEmail,
            string fromName)
        {
            var msg = new SendGridMessage
            {
                Personalizations =
                {
                    new Personalization
                    {
                        Tos = {new EmailAddress(toEmail, toName)},
                        Substitutions = substituions
                    }
                },
                From = new EmailAddress(fromEmail, fromName),
                TemplateId = templateId
            };
            var response = await _sendGridClient.SendEmailAsync(msg);
            var body = await response.Body.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                throw new Exception("send grid error: " + Environment.NewLine + body);
            }
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