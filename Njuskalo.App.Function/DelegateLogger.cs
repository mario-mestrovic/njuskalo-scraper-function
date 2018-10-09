using Library.Njuskalo;
using Microsoft.Azure.WebJobs.Host;
using System;

namespace Njuskalo.App.Function
{
    public class DelegateLogger : ILogger
    {
        private readonly TraceWriter _log;

        public DelegateLogger(TraceWriter log)
        {
            _log = log;
        }

        public void WriteLine()
        {
            // do nothing
        }
        public void WriteLine(string value)
        {
            _log.Info(value);
        }
    }
}
