using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteDownloader.Events
{
    public class FileFoundedEventArgs : EventArgs
    {
        public Uri Uri { get; set; }
        public bool IsAcceptable { get; set; }
    }
}
