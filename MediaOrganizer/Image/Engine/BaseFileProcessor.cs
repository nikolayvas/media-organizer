using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Image.Engine
{
    public interface IFileProcessor
    {
        Task CopyFileToDetinationDriveAsync(string filePath, string destinationDrive, CancellationToken cancelToken);
    }

    public abstract class BaseFileProcessor : IFileProcessor
    {
        private readonly string _destDriveRootFolder = "Galery";

        protected abstract DateTime GetOriginDateCreation(string sourcePath);

        public async Task CopyFileToDetinationDriveAsync(string filePath, string destinationDrive, CancellationToken cancelToken)
        {
            var isSpecific = filePath.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries);

            var originDate = GetOriginDateCreation(filePath);
            var destFolder = Path.Combine(destinationDrive, _destDriveRootFolder, originDate.Year.ToString("D4"), originDate.Month.ToString("D2"));
            Directory.CreateDirectory(destFolder);

            string originFileName = Path.GetFileName(filePath);
            string newFileName = originDate.ToString("yyyyMMdd");

            if(!originFileName.StartsWith(newFileName))
            {
                newFileName = string.Concat(newFileName, "_", originFileName);
            }
            else
            {
                newFileName = originFileName;
            }

            if (isSpecific.Length > 0)
            {
                newFileName = isSpecific[0] + "_" + newFileName;
            }

            var originFile = new FileInfo(filePath);
             
            var destFileName = Path.Combine(destFolder, newFileName);
            var destFile = new FileInfo(destFileName);

            if (destFile.Exists)
            {
                if(destFile.Length == originFile.Length)
                {
                    Log.Instance.Info($"Duplicated file: {originFile.FullName}");
                    return;
                }
                else
                {
                    Log.Instance.Info($"Duplicated file name but different content: {originFile.FullName}");
                    destFileName = GenerateNewSimilarName(destFile);
                }
            }

            await CopyFileAsync(originFile.FullName, destFileName, cancelToken);
        }

        private static string GenerateNewSimilarName(FileInfo destFile)
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

        public static async Task CopyFileAsync(string sourceFile, string destinationFile, CancellationToken cancelToken)
        {
            int bufferSize = 1024 * 1024;
            using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (var destinationStream = new FileStream(destinationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
                await sourceStream.CopyToAsync(destinationStream, bufferSize, cancelToken);
        }
    }
}
