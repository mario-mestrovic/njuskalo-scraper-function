using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.Email
{
    public interface IEmailSender
    {
        Task<bool> SendAsync(string to, string subject, string body, bool html);
        Task<bool> SendAsync(string from, string to, string subject, string body, bool html);

        Task<bool> SendAsync(ICollection<string> to, string subject, string body, bool html);
        Task<bool> SendAsync(string from, ICollection<string> to, string subject, string body, bool html);
    }
}
