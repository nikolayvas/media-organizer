﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Image.Engine
{
    public enum CreateFileActionEnum
    {
        None,
        CreateNew,
        Overwrite
    }

    public abstract class FileProcessor : IFileProcessor
    {
        private static readonly string _destDriveRootFolder = "Galery";
        private readonly string _dateFormat = "yyyyMMdd";
        private readonly string _dateFormat2 = "yyyy-MM-dd";
        private readonly int _bufferSize = 1024 * 1024;

        protected virtual DateTime GetOriginDateCreation(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            //yyyyMMdd_xxxxxx
            if (fileName.Length > 8 && fileName[8] == '_')
            {
                if (DateTime.TryParseExact(fileName.Substring(0, 8), _dateFormat, null, DateTimeStyles.None, out var dateCreated))
                {
                    return dateCreated;
                }
            }

            //yyyy-MM-dd xxxxxxx
            if (fileName.Length > 10 &&  fileName[4] == '-' && fileName[7] == '-' && fileName[10] == ' ')
            {
                if (DateTime.TryParseExact(fileName.Substring(0, 10), _dateFormat2, null, DateTimeStyles.None, out var dateCreated))
                {
                    return dateCreated;
                }
            }

            return DateTime.MinValue;
        }

        public virtual async Task CopyFileToDetinationDriveAsync(string filePath, string destinationDrive, CancellationToken cancelToken)
        {
            var dateTaken = GetOriginDateCreation(filePath);
            var destinationFolderPath = GetDestinationFolderPath(filePath, destinationDrive, dateTaken);
            var destFilePath = GetDestinationFilePath(filePath, destinationFolderPath, dateTaken);

            if(destFilePath.Item2 != CreateFileActionEnum.None)
            {
                //by default overwrite action
                await CopyFileAsync(filePath, destFilePath.Item1, cancelToken).ConfigureAwait(false);
            }
        }

        protected async Task CopyFileAsync(string sourceFile, string destinationFile, CancellationToken cancelToken)
        {
            using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, _bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, _bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
                await sourceStream.CopyToAsync(destinationStream, _bufferSize, cancelToken).ConfigureAwait(false);
        }

        protected string GetDestinationFolderPath(string filePath, string destinationDrive, DateTime dateTaken)
        {
            var destFolder = Path.Combine(destinationDrive, _destDriveRootFolder,
                dateTaken == DateTime.MinValue
                    ? "Unknown"
                    : dateTaken.Year.ToString("D4"), dateTaken.Month.ToString("D2")
                );

            Directory.CreateDirectory(destFolder);

            return destFolder;
        }

        protected Tuple<string, CreateFileActionEnum> GetDestinationFilePath(string filePath, string destFolder, DateTime dateTaken)
        {
            string originFileName = Path.GetFileName(filePath);

            string dateCreated = dateTaken.ToString(_dateFormat);
            string dateCreated2 = dateTaken.ToString(_dateFormat2);

            string newFileName;
            if (dateTaken != DateTime.MinValue && !originFileName.StartsWith(dateCreated) && !originFileName.StartsWith(dateCreated2))
            {
                newFileName = $"{dateCreated}_{originFileName}";
            }
            else
            {
                newFileName = originFileName;
            }

            var nestedFolder = filePath.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries);
            if (nestedFolder.Length > 1)
            {
                destFolder = Path.Combine(destFolder, nestedFolder[1]);
                Directory.CreateDirectory(destFolder);
            }

            var destFileName = Path.Combine(destFolder, newFileName).Replace("Copy of", "").Trim();

            var destFile = new FileInfo(destFileName);
            if (destFile.Exists)
            {
                var originFile = new FileInfo(filePath);
                if (destFile.Length == originFile.Length)
                {
                    Log.Instance.Info($"Duplicated file: {originFile.FullName}");
                    return new Tuple<string, CreateFileActionEnum>(destFileName, CreateFileActionEnum.None);
                }
                else
                {
                    if (destFile.Length > originFile.Length)
                    {
                        Log.Instance.Info($"Duplicated file name but different content: '{originFile.FullName}'. Overwite action!");
                        return new Tuple<string, CreateFileActionEnum>(destFileName, CreateFileActionEnum.Overwrite);
                    }
                    else
                    {
                        Log.Instance.Info($"Duplicated file name but different content: '{originFile.FullName}'. None action!");
                        return new Tuple<string, CreateFileActionEnum>(destFileName, CreateFileActionEnum.None);
                    }
                }
                /*
                else
                {
                    foreach(var newDestFileName in GetNextName(destFile))
                    {
                        destFile = new FileInfo(newDestFileName);

                        if (destFile.Exists)
                        {
                            if (destFile.Length == originFile.Length)
                            {
                                Log.Instance.Info($"Duplicated file: {originFile.FullName}");
                                return null;
                            }
                            else
                            {
                                //continue loop 
                            }
                        }
                        else
                        {
                            Log.Instance.Info($"Duplicated file name but different content: {originFile.FullName}");
                            destFileName = newDestFileName;
                            break;
                        }
                    }
                }
                */
            }

            return new Tuple<string, CreateFileActionEnum>(destFileName, CreateFileActionEnum.CreateNew);
        }

        /*
        private IEnumerable<string> GetNextName(FileInfo destFile)
        {
            string folder = Path.GetDirectoryName(destFile.FullName);
            string name = Path.GetFileNameWithoutExtension(destFile.FullName);
            string ext = destFile.Extension;

            var index = 1;
            while (true)
            {
                var newFileName = Path.Combine(folder, $"{name}_{index}{ext}");
                yield return newFileName;

                index++;
            }
        }
        */
    }
}
