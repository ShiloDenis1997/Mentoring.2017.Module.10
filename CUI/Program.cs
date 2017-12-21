using System;
using System.IO;
using System.Linq;
using CUI.Constraints;
using SiteDownloader;
using System.Collections.Generic;
using SiteDownloader.Interfaces;

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

            DirectoryInfo rootDirectory = new DirectoryInfo(options.OutputDirectoryPath);
            IContentSaver contentSaver = new ContentSaver(rootDirectory);
            ILogger logger = new SimpleLogger(options.Verbose);
            List<IConstraint> constraints = GetConstraintsFromOptions(options);
            ISiteDownloader downloader = new Downloader(logger, contentSaver, constraints, options.DeepLevel);

            try
            {
                downloader.LoadFromUrl(options.Url);
            }
            catch (Exception ex)
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
