using System;
using SiteDownloader.Events;

namespace SiteDownloader
{
    public interface ISiteDownloader
    {
        int MaxDeepLevel { get; set; }

        event EventHandler<ItemFoundedEventArgs> FileFounded;
        event EventHandler<FileLoadedEventArgs> FileLoaded;
        event EventHandler<HtmlDocumentLoadedEventArgs> HtmlLoaded;
        event EventHandler<ItemFoundedEventArgs> UrlFounded;

        void LoadFromUrl(string url);
    }
}