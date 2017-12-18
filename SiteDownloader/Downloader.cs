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
        private readonly ISet<string> _visitedUrls = new HashSet<string>();

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
                ScanUrl(httpClient, url, 0);
            }
        }

        private void ScanUrl(HttpClient httpClient, string url, int level)
        {
            if (level > MaxDeepLevel || _visitedUrls.Contains(url))
            {
                return;
            }
            _visitedUrls.Add(url);

            var urlFoundedEventArgs = new UrlFoundedEventArgs
            {
                Url = url,
                IsAcceptable = true
            };
            OnUrlFounded(urlFoundedEventArgs);
            if (!urlFoundedEventArgs.IsAcceptable)
            {
                return;
            }

            var response = httpClient.GetAsync(url).Result;
            
            if (response.IsSuccessStatusCode)
            {
                if (response.Content.Headers.ContentType.MediaType == "text/html")
                {
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
                        ScanUrl(httpClient, attributesWithLink.Value, level + 1);
                    }
                }
                else
                {
                    var fileFoundedEventArgs = new FileFoundedEventArgs
                    {
                        Uri = response.RequestMessage.RequestUri,
                        IsAcceptable = true
                    };

                    OnFileFounded(fileFoundedEventArgs);

                    if (!fileFoundedEventArgs.IsAcceptable)
                    {
                        return;
                    }

                    OnFileLoaded(new FileLoadedEventArgs
                    {
                        Uri = response.RequestMessage.RequestUri,
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
