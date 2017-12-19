using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUI.Interfaces;

namespace CUI
{
    public class SimpleLogger : ILogger
    {
        private bool _isLogEnabled;

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
