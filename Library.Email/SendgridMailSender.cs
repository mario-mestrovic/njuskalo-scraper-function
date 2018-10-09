using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Email
{
    public class SendgridMailSender : IEmailSender
    {
        private readonly SendgridMailSenderOptions _options;
        private readonly SendGridClient _client;

        public SendgridMailSender(IOptions<SendgridMailSenderOptions> options)
        {
            _options = options.Value;
            _client = new SendGridClient(_options.ApiKey);
        }

        public Task<bool> SendAsync(string to, string subject, string body, bool html)
        {
            return SendAsync(_options.EmailSender, to, subject, body, html);
        }

        public Task<bool> SendAsync(ICollection<string> to, string subject, string body, bool html)
        {
            return SendAsync(_options.EmailSender, to, subject, body, html);
        }


        public async Task<bool> SendAsync(string from, string to, string subject, string body, bool html)
        {
            if (string.IsNullOrWhiteSpace(from))
                throw new ArgumentNullException(nameof(from));
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentNullException(nameof(to));

            var emailFrom = from != null ? new EmailAddress(from) : null;
            var emailTo = new EmailAddress(to);
            try
            {
                var msg = MailHelper.CreateSingleEmail(emailFrom, emailTo, subject, body, html ? body : null);
                var response = await _client.SendEmailAsync(msg);
                // For better response handling visit https://sendgrid.com/docs/API_Reference/Web_API_v3/Mail/errors.html
                if ((int)response.StatusCode >= 200 && ((int)response.StatusCode <= 299))
                {
                    //_logger.Debug("Email sent successfully");
                    return true;
                }
                else
                {
                    var content = await response.Body.ReadAsStringAsync();
                    //_logger.Error($"Error sending email: \nStatusCode: {response.StatusCode}\nResponse: {content}");
                    return false;
                }
            }
            catch (Exception)
            {
                //_logger.Error("Error sending email", exception);
                return false;
            }
        }

        public async Task<bool> SendAsync(string from, ICollection<string> to, string subject, string body, bool html)
        {
            //if (string.IsNullOrWhiteSpace(from))
            //    throw new ArgumentNullException(nameof(from));
            if (to?.Any() != true)
                throw new ArgumentNullException(nameof(to));

            var emailFrom = from != null ? new EmailAddress(from) : null;
            var emailTo = to.Select(x => new EmailAddress(x)).ToList();
            var subjects = to.Select(x => subject).ToList();
            var subs = to.Select(x => new Dictionary<string, string>()).ToList();
            try
            {
                var msg = MailHelper.CreateMultipleEmailsToMultipleRecipients(emailFrom, emailTo, subjects, body, html ? body : null, subs);
                var response = await _client.SendEmailAsync(msg);
                // For better response handling visit https://sendgrid.com/docs/API_Reference/Web_API_v3/Mail/errors.html
                if ((int)response.StatusCode >= 200 && ((int)response.StatusCode <= 299))
                {
                    //_logger.Debug("Email sent successfully");
                    return true;
                }
                else
                {
                    var content = await response.Body.ReadAsStringAsync();
                    //_logger.Error($"Error sending email: \nStatusCode: {response.StatusCode}\nResponse: {content}");
                    return false;
                }
            }
            catch (Exception)
            {
                //_logger.Error("Error sending email", exception);
                return false;
            }
        }

    }
}
