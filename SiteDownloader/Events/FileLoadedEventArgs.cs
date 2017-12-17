using System;
using System.IO;

namespace SiteDownloader.Events
{
    public class FileLoadedEventArgs : EventArgs
    {
        public Uri Uri { get; set; }
        public Stream FileContent { get; set; }
    }
}
