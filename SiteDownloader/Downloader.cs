using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SiteDownloader.Events;

namespace SiteDownloader
{
    public class Downloader
    {
        private readonly ISet<Uri> _visitedUrls = new HashSet<Uri>();

        public event EventHandler<UrlFoundedEventArgs> UrlFounded;
        public event EventHandler<HtmlDocumentLoadedEventArgs> HtmlLoaded;
        public event EventHandler<FileFoundedEventArgs> FileFounded;
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

        public void LoadFromUrl(string url)
        {
            _visitedUrls.Clear();
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(url);
                ScanUrl(httpClient, httpClient.BaseAddress, 0);
            }
        }

        private void ScanUrl(HttpClient httpClient, Uri uri, int level)
        {
            if (level > MaxDeepLevel || _visitedUrls.Contains(uri))
            {
                return;
            }
            _visitedUrls.Add(uri);

            var urlFoundedEventArgs = new UrlFoundedEventArgs
            {
                Uri = uri,
                IsAcceptable = true
            };
            OnUrlFounded(urlFoundedEventArgs);
            if (!urlFoundedEventArgs.IsAcceptable)
            {
                return;
            }
            
            var head = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri)).Result;
            
            if (head.IsSuccessStatusCode)
            {
                if (head.Content.Headers.ContentType.MediaType == "text/html")
                {
                    var response = httpClient.GetAsync(uri).Result;
                    var document = new HtmlDocument();
                    document.Load(response.Content.ReadAsStreamAsync().Result,
                        Encoding.UTF8);
                    OnHtmlLoaded(new HtmlDocumentLoadedEventArgs
                    {
                        Uri = response.RequestMessage.RequestUri,
                        Document = document
                    });
                    var attributesWithLinks = document.DocumentNode.Descendants()
                        .SelectMany(d => d.Attributes.Where(CheckIfAttributeHaveLink));
                    foreach (var attributesWithLink in attributesWithLinks)
                    {
                        ScanUrl(httpClient, new Uri(httpClient.BaseAddress, attributesWithLink.Value), level + 1);
                    }
                }
                else
                {
                    var fileFoundedEventArgs = new FileFoundedEventArgs
                    {
                        Uri = uri,
                        IsAcceptable = true
                    };

                    OnFileFounded(fileFoundedEventArgs);

                    if (!fileFoundedEventArgs.IsAcceptable)
                    {
                        return;
                    }

                    var response = httpClient.GetAsync(uri).Result;
                    OnFileLoaded(new FileLoadedEventArgs
                    {
                        Uri = uri,
                        FileContent = response.Content.ReadAsStreamAsync().Result
                    });
                }
            }
        }

        private bool CheckIfAttributeHaveLink(HtmlAttribute attribute)
        {
            return attribute.Name == "src" || attribute.Name == "href";
        }

        private void OnUrlFounded(UrlFoundedEventArgs args)
        {
            UrlFounded?.Invoke(this, args);
        }

        private void OnHtmlLoaded(HtmlDocumentLoadedEventArgs args)
        {
            HtmlLoaded?.Invoke(this, args);
        }

        private void OnFileFounded(FileFoundedEventArgs args)
        {
            FileFounded?.Invoke(this, args);
        }

        private void OnFileLoaded(FileLoadedEventArgs args)
        {
            FileLoaded?.Invoke(this, args);
        }
    }
}
