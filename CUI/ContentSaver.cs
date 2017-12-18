using System;
using System.IO;
using System.Linq;
using HtmlAgilityPack;

namespace CUI
{
    public class ContentSaver
    {
        public void SaveHtmlDocument(DirectoryInfo rootDirectory, Uri uri, string name, Stream documentStream)
        {
            string directoryPath = CombineLocations(rootDirectory, uri);
            Directory.CreateDirectory(directoryPath);
            name = RemoveInvalidSymbols(name);
            string fileFullPath = Path.Combine(directoryPath, name);

            var createdFileStream = File.Create(fileFullPath);
            documentStream.CopyTo(createdFileStream);
            documentStream.Close();
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
            fileStream.Close();
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
