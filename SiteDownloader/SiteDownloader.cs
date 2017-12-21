using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;
using SiteDownloader.Interfaces;

namespace SiteDownloader
{
    public class Downloader : ISiteDownloader
    {
        private readonly ILogger _logger;
        private readonly IContentSaver _contentSaver;
        private readonly List<IConstraint> _urlConstraints;
        private readonly List<IConstraint> _fileConstraints;
        private const string HtmlDocumentMediaType = "text/html";
        private readonly ISet<Uri> _visitedUrls = new HashSet<Uri>();

        public int MaxDeepLevel { get; set; }

        public Downloader(ILogger logger, IContentSaver contentSaver, IEnumerable<IConstraint> constraints, int maxDeepLevel = 0)
        {
            if (maxDeepLevel < 0)
            {
                throw new ArgumentException($"{nameof(maxDeepLevel)} cannot be less than zero");
            }

            _logger = logger;
            _contentSaver = contentSaver;
            _urlConstraints = constraints.Where(c => (c.ConstraintType & ConstraintType.UrlConstraint) != 0).ToList();
            _fileConstraints = constraints.Where(c => (c.ConstraintType & ConstraintType.FileConstraint) != 0).ToList();

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
            if (level > MaxDeepLevel || _visitedUrls.Contains(uri) || !IsValidScheme(uri.Scheme))
            {
                return;
            }
            _visitedUrls.Add(uri);
            HttpResponseMessage head = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri)).Result;

            if (!head.IsSuccessStatusCode)
            {
                return;
            }

            if (head.Content.Headers.ContentType?.MediaType == HtmlDocumentMediaType)
            {
                ProcessHtmlDocument(httpClient, uri, level);
            }
            else
            {
                ProcessFile(httpClient, uri);
            }
        }
        private void ProcessFile(HttpClient httpClient, Uri uri)
        {
            _logger.Log($"File founded: {uri}");
            if (!IsAcceptableUri(uri, _fileConstraints))
            {
                return;
            }

            var response = httpClient.GetAsync(uri).Result;
            _logger.Log($"File loaded: {uri}");
            _contentSaver.SaveFile(uri, response.Content.ReadAsStreamAsync().Result);
        }

        private void ProcessHtmlDocument(HttpClient httpClient, Uri uri, int level)
        {
            _logger.Log($"Url founded: {uri}");
            if (!IsAcceptableUri(uri, _urlConstraints))
            {
                return;
            }

            var response = httpClient.GetAsync(uri).Result;
            var document = new HtmlDocument();
            document.Load(response.Content.ReadAsStreamAsync().Result, Encoding.UTF8);
            _logger.Log($"Html loaded: {uri}");
            _contentSaver.SaveHtmlDocument(uri, GetDocumentFileName(document), GetDocumentStream(document));

            var attributesWithLinks = document.DocumentNode.Descendants().SelectMany(d => d.Attributes.Where(IsAttributeWithLink));
            foreach (var attributesWithLink in attributesWithLinks)
            {
                ScanUrl(httpClient, new Uri(httpClient.BaseAddress, attributesWithLink.Value), level + 1);
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

        private bool IsAttributeWithLink(HtmlAttribute attribute)
        {
            return attribute.Name == "src" || attribute.Name == "href";
        }

        private bool IsAcceptableUri(Uri uri, IEnumerable<IConstraint> constraints)
        {
            return constraints.All(c => c.IsAcceptable(uri));
        }
    }
}
