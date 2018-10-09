using Microsoft.Extensions.Logging;

namespace Njuskalo.App.Function
{
    public class DelegateLogger : Library.Njuskalo.ILogger
    {
        private readonly ILogger _logger;

        public DelegateLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void WriteLine()
        {
            WriteLine(null);

        }
        public void WriteLine(string value)
        {
            if (value != null) _logger.LogInformation(value);
        }
    }
}
