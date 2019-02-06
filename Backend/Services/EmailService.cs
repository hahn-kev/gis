using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Backend.Entities;
using Backend.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Backend.Services
{
    public interface IEmailService
    {
        Task SendTemplateEmail(Dictionary<string, string> substitutions,
            string subject,
            EmailTemplate emailTemplate,
            PersonWithStaff from,
            PersonWithStaff to);

        Task SendTemplateEmail(Dictionary<string, string> substitutions,
            string subject,
            EmailTemplate emailTemplate,
            PersonWithStaff from,
            IEnumerable<PersonWithStaff> tos);

        Task SendEmail(SendGridMessage message);
    }

    public class EmailService : IEmailService
    {
        private readonly SendGridClient _sendGridClient;
        private readonly TemplateSettings _templateSettings;

        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<Settings> options,
            IOptions<TemplateSettings> templateSettings,
            ILogger<EmailService> logger)
        {
            _logger = logger;
            _sendGridClient = new SendGridClient(options.Value.SendGridAPIKey);
            _templateSettings = templateSettings.Value;
        }

        public virtual Task SendTemplateEmail(Dictionary<string, string> substitutions,
            string subject,
            EmailTemplate emailTemplate,
            PersonWithStaff @from,
            PersonWithStaff to)
        {
            return SendTemplateEmail(substitutions,
                subject,
                emailTemplate,
                from.Staff.Email,
                from.PreferredName,
                to.Staff.Email,
                to.PreferredName);
        }

        public virtual Task SendTemplateEmail(Dictionary<string, string> substitutions,
            string subject,
            EmailTemplate emailTemplate,
            PersonWithStaff @from,
            IEnumerable<PersonWithStaff> tos)
        {
            return SendTemplateEmail(substitutions,
                subject,
                emailTemplate,
                from.Staff.Email,
                from.PreferredName,
                tos.Select(person => new EmailAddress(person.Staff.Email, person.PreferredName)).ToList());
        }

        public virtual Task SendTemplateEmail(Dictionary<string, string> substitutions,
            string subject,
            EmailTemplate emailTemplate,
            PersonExtended from,
            PersonExtended to)
        {
            return SendTemplateEmail(substitutions,
                subject,
                emailTemplate,
                from.Email,
                from.PreferredName,
                to.Email,
                to.PreferredName);
        }

        public virtual Task SendTemplateEmail(Dictionary<string, string> substitutions,
            string subject,
            EmailTemplate emailTemplate,
            string fromEmail,
            string fromName,
            string toEmail,
            string toName)
        {
            if (toEmail == null)
                throw new ArgumentNullException(nameof(toEmail), $"{toName} does not have an email assigned");
            return SendTemplateEmail(substitutions,
                subject,
                emailTemplate,
                fromEmail,
                fromName,
                new List<EmailAddress> {new EmailAddress(toEmail, toName)});
        }

        public virtual Task SendTemplateEmail(Dictionary<string, string> substitutions,
            string subject,
            EmailTemplate emailTemplate,
            string fromEmail,
            string fromName,
            List<EmailAddress> tos)
        {
            foreach (var emailAddress in tos)
            {
                if (string.IsNullOrEmpty(emailAddress.Email))
                    throw new ArgumentNullException(nameof(emailAddress.Email),
                        $"{emailAddress.Name} does not have an email assigned");
            }

            if (!tos.Any()) return Task.CompletedTask;
            fromEmail = string.IsNullOrEmpty(fromEmail) ? "dont-reply@gisthailand.org" : fromEmail;
            var msg = new SendGridMessage
            {
                Personalizations = new List<Personalization>
                {
                    new Personalization
                    {
                        Tos = tos,
                        TemplateData = substitutions
                    }
                },
                From = new EmailAddress(fromEmail, fromName),
                Subject = subject,
                TemplateId = emailTemplate.GetId(_templateSettings)
            };
            return SendEmail(msg);
        }

        //this method gets replaced by moq, if it's overloaded that needs to be accounted for
        public virtual async Task SendEmail(SendGridMessage message)
        {
#if DEBUG
            var to =
 string.Join(", ", message.Personalizations.Select(personalization => string.Join(", ", personalization.Tos.Select(address => $"{address.Name}:{address.Email}"))));
            _logger.LogDebug("email not sent to: [{0}], subject: {1}, because of debugging", to, message.Subject);
            return;
#endif
            var response = await _sendGridClient.SendEmailAsync(message);
            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                var body = await response.Body.ReadAsStringAsync();
                throw new Exception("send grid error: " + Environment.NewLine + body);
            }
        }
    }
}