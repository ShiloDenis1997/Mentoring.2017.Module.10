using System;
using SiteDownloader.Interfaces;

namespace CUI
{
    public class SimpleLogger : ILogger
    {
        private readonly bool _isLogEnabled;

        public SimpleLogger(bool isLogEnabled)
        {
            _isLogEnabled = isLogEnabled;
        }

        public void Log(string message)
        {
            if (_isLogEnabled)
            {
                Console.WriteLine(message);
            }
        }
    }
}
