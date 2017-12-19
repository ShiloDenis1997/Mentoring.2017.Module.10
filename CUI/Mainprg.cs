using System;
using System.IO;
using System.Linq;
using CUI.Constraints;
using CUI.Interfaces;
using SiteDownloader;
using System.Collections.Generic;
using CUI.Enums;

namespace CUI
{
    class Mainprg
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                return;
            }

            DirectoryInfo rootDirectory = new DirectoryInfo(options.OutputDirectoryPath);
            ISiteDownloader downloader = new Downloader(options.DeepLevel);
            IContentSaver contentSaver = new ContentSaver(rootDirectory);
            ILogger logger = new SimpleLogger(options.Verbose);
            List<IConstraint> constraints = GetConstraintsFromOptions(options);
            var urlConstraints = constraints.Where(c => (c.ConstraintType & ConstraintType.UrlConstraint) != 0).ToList();
            var fileConstraints = constraints.Where(c => (c.ConstraintType & ConstraintType.FileConstraint) != 0).ToList();

            downloader.UrlFounded += (sender, eventArgs) =>
            {
                logger.Log($"Url founded: {eventArgs.Uri}");
                eventArgs.IsAcceptable = urlConstraints.All(c => c.IsAcceptable(eventArgs.Uri));
            };

            downloader.FileFounded += (sender, eventArgs) =>
            {
                logger.Log($"File founded: {eventArgs.Uri}");
                eventArgs.IsAcceptable = fileConstraints.All(c => c.IsAcceptable(eventArgs.Uri));
            };

            downloader.FileLoaded += (sender, eventArgs) =>
            {
                logger.Log($"File loaded: {eventArgs.Uri}");
                contentSaver.SaveFile(eventArgs.Uri, eventArgs.FileContent);
            };

            downloader.HtmlLoaded += (sender, eventArgs) =>
            {
                logger.Log($"Html loaded: {eventArgs.Uri}");
                contentSaver.SaveHtmlDocument(eventArgs.Uri, eventArgs.FileName, eventArgs.Document);
            };

            try
            {
                downloader.LoadFromUrl(options.Url).Wait();
            }
            catch (AggregateException ex)
            {
                logger.Log($"Error during site downloading: {ex.Message}");
            }
        }

        public static List<IConstraint> GetConstraintsFromOptions(Options options)
        {
            List<IConstraint> constraints = new List<IConstraint>();
            if (options.AvailableExtensions != null)
            {
                constraints.Add(new FileTypesConstraint(options.AvailableExtensions.Split(',').Select(e => "." + e)));
            }

            constraints.Add(new CrossDomainTransitionConstraint(options.CrossDomainTransition, new Uri(options.Url)));

            return constraints;
        }
    }
}
