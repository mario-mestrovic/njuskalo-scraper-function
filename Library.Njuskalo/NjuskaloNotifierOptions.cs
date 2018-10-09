using System.Collections.Generic;
using System.Linq;

namespace Library.Njuskalo
{
    public class NjuskaloNotifierOptions
    {
        public string Emails { get; set; }

        public List<string> GetEmails()
        {
            return Emails?.Split(';').Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
    }
}
