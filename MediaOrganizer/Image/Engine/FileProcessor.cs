using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Image.Engine
{
    public abstract class FileProcessor : IFileProcessor
    {
        private static readonly string _destDriveRootFolder = "Galery";
        private readonly string _dateFormat = "yyyyMMdd";
        private readonly int _bufferSize = 1024 * 1024;

        protected abstract DateTime GetOriginDateCreation(string sourcePath);

        public virtual async Task CopyFileToDetinationDriveAsync(string filePath, string destinationDrive, CancellationToken cancelToken)
        {
            var dateTaken = GetOriginDateCreation(filePath);
            var destinationFolderPath = GetDestinationFolderPath(filePath, destinationDrive, dateTaken);
            var destFilePath = GetDestinationFilePath(filePath, destinationFolderPath, dateTaken);

            if(destFilePath != null)
            {
                await CopyFileAsync(filePath, destFilePath, cancelToken);
            }
        }

        protected async Task CopyFileAsync(string sourceFile, string destinationFile, CancellationToken cancelToken)
        {
            using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, _bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (var destinationStream = new FileStream(destinationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, _bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
                await sourceStream.CopyToAsync(destinationStream, _bufferSize, cancelToken);
        }

        protected string GetDestinationFolderPath(string filePath, string destinationDrive, DateTime dateTaken)
        {
            var destFolder = Path.Combine(destinationDrive, _destDriveRootFolder, dateTaken.Year.ToString("D4"), dateTaken.Month.ToString("D2"));
            Directory.CreateDirectory(destFolder);

            return destFolder;
        }

        protected string GetDestinationFilePath(string filePath, string destFolder, DateTime dateTaken)
        {
            var isSpecific = filePath.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries);

            string originFileName = Path.GetFileName(filePath);
            string newFileName = dateTaken.ToString(_dateFormat);

            if (!originFileName.StartsWith(newFileName))
            {
                newFileName = string.Concat(newFileName, "_", originFileName);
            }
            else
            {
                newFileName = originFileName;
            }

            if (isSpecific.Length > 1)
            {
                newFileName = isSpecific[1] + "_" + newFileName;
            }

            var destFileName = Path.Combine(destFolder, newFileName);
            var destFile = new FileInfo(destFileName);

            if (destFile.Exists)
            {
                var originFile = new FileInfo(filePath);
                if (destFile.Length == originFile.Length)
                {
                    Log.Instance.Info($"Duplicated file: {originFile.FullName}");
                    return null;
                }
                else
                {
                    Log.Instance.Info($"Duplicated file name but different content: {originFile.FullName}");
                    destFileName = GenerateNewSimilarName(destFile);
                }
            }

            return destFileName;
        }

        private string GenerateNewSimilarName(FileInfo destFile)
        {
            string folder = Path.GetDirectoryName(destFile.FullName);
            string name = Path.GetFileNameWithoutExtension(destFile.FullName);
            string ext = destFile.Extension;

            string suffix = "_";
            while (true)
            {
                var newFileName = Path.Combine(folder, name + suffix + ext);
                var file = new FileInfo(newFileName);

                if (!file.Exists)
                {
                    return newFileName;
                }

                suffix += "_";
            }
        }
    }
}
