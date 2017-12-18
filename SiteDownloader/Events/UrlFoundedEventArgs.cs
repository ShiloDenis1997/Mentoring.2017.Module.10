using System;

namespace SiteDownloader.Events
{
    public class UrlFoundedEventArgs : EventArgs
    {
        public Uri Uri { get; set; }
        public bool IsAcceptable { get; set; }
    }
}
