using System;
using System.IO;

namespace SiteDownloader.Interfaces
{
    public interface IContentSaver
    {
        void SaveFile(Uri uri, Stream fileStream);
        void SaveHtmlDocument(Uri uri, string name, Stream documentStream);
    }
}