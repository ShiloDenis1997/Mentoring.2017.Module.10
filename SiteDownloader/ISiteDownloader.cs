using System;
using System.Threading.Tasks;
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

        Task LoadFromUrl(string url);
    }
}