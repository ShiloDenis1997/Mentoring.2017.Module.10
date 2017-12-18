using System;
using System.IO;
using System.Linq;
using CUI.Constraints;
using SiteDownloader;

namespace CUI
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                return;
            }

            Console.WriteLine(options.Verbose);

            ISiteDownloader downloader = new Downloader(options.DeepLevel);
            ContentSaver contentSaver = new ContentSaver();
            DirectoryInfo rootDirectory = new DirectoryInfo(options.OutputDirectoryPath);
            rootDirectory.Create();

            FileTypesConstraint fileTypesConstraint = null;
            if (options.AvailableExtensions != null)
            {
                fileTypesConstraint = new FileTypesConstraint(options.AvailableExtensions.Split(',').Select(e => "." + e));
            }
            else
            {
                Console.WriteLine(options.GetUsage());
                return;
            }

            CrossDomainTransitionConstraint crossDomainTransitionConstraint = new CrossDomainTransitionConstraint(options.CrossDomainTransition, new Uri(options.Url));

            downloader.UrlFounded += (sender, eventArgs) =>
            {
                if (options.Verbose)
                {
                    Console.WriteLine(eventArgs.Uri);
                }
                eventArgs.IsAcceptable = crossDomainTransitionConstraint.IsAcceptable(eventArgs.Uri);
            };

            downloader.FileFounded += (sender, eventArgs) =>
            {
                Console.WriteLine(eventArgs.Uri);
                eventArgs.IsAcceptable = fileTypesConstraint.IsAcceptable(eventArgs.Uri);
            };

            downloader.FileLoaded += (sender, eventArgs) =>
            {
                Console.WriteLine($"File: {eventArgs.Uri}");
                contentSaver.SaveFile(rootDirectory, eventArgs.Uri, eventArgs.FileContent);
            };

            downloader.HtmlLoaded += (sender, eventArgs) =>
            {
                contentSaver.SaveHtmlDocument(rootDirectory, eventArgs.Uri, eventArgs.FileName, eventArgs.Document);
            };
            
            downloader.LoadFromUrl(options.Url);
        }
    }
}
