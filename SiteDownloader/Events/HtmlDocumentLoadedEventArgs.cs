using System;
using HtmlAgilityPack;

namespace SiteDownloader.Events
{
    public class HtmlDocumentLoadedEventArgs : EventArgs
    {
        public Uri Uri { get; set; }
        public HtmlDocument Document { get; set; }
    }
}
