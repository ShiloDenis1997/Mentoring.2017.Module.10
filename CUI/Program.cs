using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiteDownloader;

namespace CUI
{
    class Program
    {
        static void Main(string[] args)
        {
            Downloader downloader = new Downloader(2);
            ContentSaver contentSaver = new ContentSaver();
            DirectoryInfo rootDirectory = new DirectoryInfo(@"D:\websites\stack");
            rootDirectory.Create();

            downloader.UrlFounded += (sender, eventArgs) => { Console.WriteLine(eventArgs.Url); };
            downloader.FileLoaded += (sender, eventArgs) =>
            {
                Console.WriteLine($"File: {eventArgs.Uri}");
                contentSaver.SaveFile(rootDirectory, eventArgs.Uri, eventArgs.FileContent);
            };

            downloader.HtmlLoaded += (sender, eventArgs) =>
            {
                Console.WriteLine($"Html: {eventArgs.Uri}");
                contentSaver.SaveHtmlDocument(rootDirectory, eventArgs.Uri, eventArgs.Document);
            };
            
            Console.WriteLine("Started");
            downloader.LoadFromUrl("https://ru.stackoverflow.com/questions/420354/%D0%9A%D0%B0%D0%BA-%D1%80%D0%B0%D1%81%D0%BF%D0%B0%D1%80%D1%81%D0%B8%D1%82%D1%8C-html-%D0%B2-net/450586");
            Console.WriteLine("Finished");
        }
    }
}
