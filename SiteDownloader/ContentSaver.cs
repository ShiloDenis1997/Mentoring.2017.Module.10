using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace SiteDownloader
{
    public class ContentSaver
    {
        public void SaveHtmlDocument(DirectoryInfo rootDirectory, Uri uri, HtmlDocument document)
        {
            string directoryPath = CombineLocations(rootDirectory, uri);
            Directory.CreateDirectory(directoryPath);
            string filename = document.DocumentNode.Descendants("title").FirstOrDefault()?.InnerText + ".html";
            filename = RemoveInvalidSymbols(filename);
            string fileFullPath = Path.Combine(directoryPath, filename);
            var createdFileStream = File.Create(fileFullPath);
            document.Save(createdFileStream);
            createdFileStream.Close();
        }

        public void SaveFile(DirectoryInfo rootDirectory, Uri uri, Stream fileStream)
        {
            string fileFullPath = CombineLocations(rootDirectory, uri);
            var directoryPath = Path.GetDirectoryName(fileFullPath);
            Directory.CreateDirectory(directoryPath);
            if (Directory.Exists(fileFullPath))
            {
                fileFullPath = Path.Combine(fileFullPath, Guid.NewGuid().ToString());
            }
            var createdFileStream = File.Create(fileFullPath);
            fileStream.CopyTo(createdFileStream);
            createdFileStream.Close();
        }

        private string CombineLocations(DirectoryInfo directory, Uri uri)
        {
            return Path.Combine(directory.FullName, uri.Host) + uri.LocalPath.Replace("/", @"\");
        }

        private string RemoveInvalidSymbols(string filename)
        {
            var invalidSymbols = Path.GetInvalidFileNameChars();
            return new string(filename.Where(c => !invalidSymbols.Contains(c)).ToArray());
        }
    }
}
