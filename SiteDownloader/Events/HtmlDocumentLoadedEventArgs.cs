using System;
using System.IO;

namespace SiteDownloader.Events
{
    public class HtmlDocumentLoadedEventArgs : EventArgs
    {
        public Uri Uri { get; set; }
        public string FileName { get; set; }
        public Stream Document { get; set; }
    }
}
