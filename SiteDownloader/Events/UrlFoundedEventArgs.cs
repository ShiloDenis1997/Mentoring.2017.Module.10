using System;

namespace SiteDownloader.Events
{
    public class UrlFoundedEventArgs : EventArgs
    {
        public string Url { get; set; }
        public bool IsAcceptable { get; set; }
    }
}
