﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
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

            Downloader downloader = new Downloader(options.DeepLevel);
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
                    Console.WriteLine(new Uri(new Uri(options.Url), eventArgs.Url));
                }
                eventArgs.IsAcceptable = crossDomainTransitionConstraint.IsAcceptable(new Uri(new Uri(options.Url), eventArgs.Url));
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
                //Console.WriteLine($"Html: {eventArgs.Uri}");
                contentSaver.SaveHtmlDocument(rootDirectory, eventArgs.Uri, eventArgs.Document);
            };
            
            downloader.LoadFromUrl(options.Url);
        }
    }
}
