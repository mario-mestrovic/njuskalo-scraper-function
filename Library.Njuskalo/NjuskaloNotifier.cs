using Library.Email;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Library.Njuskalo
{
    public class NjuskaloNotifier
    {
        private readonly NjuskaloNotifierOptions _options;
        private readonly IEmailSender _emailSender;

        public NjuskaloNotifier(IOptions<NjuskaloNotifierOptions> options, IEmailSender emailSender)
        {
            _options = options.Value;
            _emailSender = emailSender;
        }

        public async Task<bool> NotifyAboutEntitiesAsync(ICollection<string> entities)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<p>Hi!<br/>Found {entities.Count} new ads:</p>");
            foreach (var url in entities)
            {
                sb.AppendLine($"<p><a href=\"{url}\">{url}</a></p>");
            }

            var nzTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, nzTimeZone);

            var subject = $"[njuskalo] {nowLocal.ToString("yyyy-MM-dd HH:mm")} Found {entities.Count} ads.";
            return await _emailSender.SendAsync(_options.GetEmails(), subject, sb.ToString(), true);
        }
    }
}
