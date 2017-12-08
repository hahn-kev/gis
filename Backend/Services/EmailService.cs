using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
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
            string subject,
            EmailService.Template template,
            PersonExtended from,
            PersonExtended to);

        Task SendTemplateEmail(Dictionary<string, string> substituions,
            string subject,
            EmailService.Template template,
            string fromEmail,
            string fromName,
            string toEmail,
            string toName);
    }

    public class EmailService : IEmailService
    {
        private static readonly IMailGunApi MailGunApi = RestClient.For<IMailGunApi>("https://api.mailgun.net/v3");
        private readonly SendGridClient _sendGridClient;
        private readonly string _domain;

        public struct Template
        {
            private Template(string id, [CallerMemberName] string name = null)
            {
                Id = id;
                Name = name;
            }

            public string Id { get; }
            public string Name { get; }
            public static Template NotifyLeaveRequest => new Template("na");
            public static Template RequestLeaveApproval => new Template("70b6165d-f367-401f-9ae4-56814033b720");

            public bool Equals(Template other)
            {
                return string.Equals(Id, other.Id);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is Template && Equals((Template) obj);
            }

            public static bool operator ==(Template a, Template b)
            {
                return object.Equals(a, b);
            }

            public static bool operator !=(Template a, Template b)
            {
                return !(a == b);
            }

            public override int GetHashCode()
            {
                return (Id != null ? Id.GetHashCode() : 0);
            }

            public override string ToString()
            {
                return Name;
            }
        }

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
            string subject,
            Template template,
            PersonExtended from,
            PersonExtended to)
        {
            return SendTemplateEmail(substituions,
                subject,
                template,
                from.Email,
                from.PreferredName,
                to.Email,
                to.PreferredName);
        }

        public async Task SendTemplateEmail(Dictionary<string, string> substituions,
            string subject,
            Template template,
            string fromEmail,
            string fromName,
            string toEmail,
            string toName)
        {
            if (toEmail == null) throw new ArgumentNullException(nameof(toEmail));
            if (fromEmail == null) throw new ArgumentNullException(nameof(fromEmail));
            var msg = new SendGridMessage
            {
                Personalizations = new List<Personalization>
                {
                    new Personalization
                    {
                        Tos = new List<EmailAddress> {new EmailAddress(toEmail, toName)},
                        Substitutions = substituions
                    }
                },
                From = new EmailAddress(fromEmail, fromName),
                Subject = subject,
                TemplateId = template.Id
            };
            var response = await _sendGridClient.SendEmailAsync(msg);
            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                var body = await response.Body.ReadAsStringAsync();
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