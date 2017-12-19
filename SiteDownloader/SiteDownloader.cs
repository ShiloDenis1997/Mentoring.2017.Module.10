using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SiteDownloader.Events;

namespace SiteDownloader
{
    public class Downloader : ISiteDownloader
    {
        private const string HtmlDocumentMediaType = "text/html";
        private readonly ISet<Uri> _visitedUrls = new HashSet<Uri>();

        public event EventHandler<ItemFoundedEventArgs> UrlFounded;
        public event EventHandler<HtmlDocumentLoadedEventArgs> HtmlLoaded;
        public event EventHandler<ItemFoundedEventArgs> FileFounded;
        public event EventHandler<FileLoadedEventArgs> FileLoaded;

        public int MaxDeepLevel { get; set; }

        public Downloader(int maxDeepLevel = 0)
        {
            if (maxDeepLevel < 0)
            {
                throw new ArgumentException($"{nameof(maxDeepLevel)} cannot be less than zero");
            }

            MaxDeepLevel = maxDeepLevel;
        }

        public async Task LoadFromUrl(string url)
        {
            _visitedUrls.Clear();
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(url);
                await ScanUrl(httpClient, httpClient.BaseAddress, 0);
            }
        }

        private async Task ScanUrl(HttpClient httpClient, Uri uri, int level)
        {
            if (level > MaxDeepLevel || _visitedUrls.Contains(uri) || !IsValidScheme(uri.Scheme))
            {
                return;
            }
            _visitedUrls.Add(uri);
            Console.WriteLine(uri);
            HttpResponseMessage head = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));

            if (!head.IsSuccessStatusCode)
            {
                return;
            }

            if (head.Content.Headers.ContentType?.MediaType == HtmlDocumentMediaType)
            {
                await ProcessHtmlDocument(httpClient, uri, level);
            }
            else
            {
                await ProcessFile(httpClient, uri);
            }
        }

        private bool IsValidScheme(string scheme)
        {
            switch (scheme)
            {
                case "http":
                case "https":
                    return true;
                default:
                    return false;
            }
        }

        private async Task ProcessFile(HttpClient httpClient, Uri uri)
        {
            var fileFoundedEventArgs = OnItemFounded(FileFounded, uri);
            if (!fileFoundedEventArgs.IsAcceptable)
            {
                return;
            }

            var response = httpClient.GetAsync(uri).Result;
            OnFileLoaded(new FileLoadedEventArgs
            {
                Uri = uri,
                FileContent = await response.Content.ReadAsStreamAsync()
            });
        }

        private async Task ProcessHtmlDocument(HttpClient httpClient, Uri uri, int level)
        {
            var urlFoundedEventArgs = OnItemFounded(UrlFounded, uri);
            if (!urlFoundedEventArgs.IsAcceptable)
            {
                return;
            }

            var response = await httpClient.GetAsync(uri);
            var document = new HtmlDocument();
            document.Load(await response.Content.ReadAsStreamAsync(), Encoding.UTF8);
            OnHtmlLoaded(new HtmlDocumentLoadedEventArgs
            {
                Uri = uri,
                FileName = GetDocumentFileName(document),
                Document = GetDocumentStream(document)
            });

            var attributesWithLinks = document.DocumentNode.Descendants()
                .SelectMany(d => d.Attributes.Where(IsAttributeWithLink));
            foreach (var attributesWithLink in attributesWithLinks)
            {
                await ScanUrl(httpClient, new Uri(httpClient.BaseAddress, attributesWithLink.Value), level + 1);
            }
        }

        private string GetDocumentFileName(HtmlDocument document)
        {
            return document.DocumentNode.Descendants("title").FirstOrDefault()?.InnerText + ".html";
        }

        private Stream GetDocumentStream(HtmlDocument document)
        {
            MemoryStream memoryStream = new MemoryStream();
            document.Save(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        private bool IsAttributeWithLink(HtmlAttribute attribute)
        {
            return attribute.Name == "src" || attribute.Name == "href";
        }

        private ItemFoundedEventArgs OnItemFounded(EventHandler<ItemFoundedEventArgs> eventHandler, Uri uri)
        {
            ItemFoundedEventArgs eventArgs = new ItemFoundedEventArgs
            {
                Uri = uri,
                IsAcceptable = true
            };
            eventHandler?.Invoke(this, eventArgs);
            return eventArgs;
        }

        private void OnHtmlLoaded(HtmlDocumentLoadedEventArgs args)
        {
            HtmlLoaded?.Invoke(this, args);
        }

        private void OnFileLoaded(FileLoadedEventArgs args)
        {
            FileLoaded?.Invoke(this, args);
        }
    }
}
