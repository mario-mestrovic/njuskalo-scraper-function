﻿using Library.Njuskalo;
using System;

namespace Njuskalo.App.Console
{
    public class DelegateLogger : ILogger
    {
        private readonly Action<string> _logMethod;

        public DelegateLogger(Action<string> logMethod)
        {
            _logMethod = logMethod;
        }

        public void WriteLine()
        {
            WriteLine(null);

        }
        public void WriteLine(string value)
        {
            _logMethod(value);
        }
    }
}
