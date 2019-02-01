﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Image.Engine
{
    public class JVCMovieFileProcessor : FileProcessor
    {
        private const string MOD_EXT = ".mod";
        private const string MOI_EXT = ".moi";

        protected override DateTime GetOriginDateCreation(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    int hexIn;
                    string hex;

                    string yearHex = "";

                    int year, month = 0, day = 0, hour = 0, min = 0, sec = 0;

                    for (int i = 0; (hexIn = fs.ReadByte()) != -1; i++)
                    {
                        hex = string.Format("{0:X2}", hexIn);

                        if (i == 6 || i == 7)
                        {
                            yearHex += hex;

                            continue;
                        }

                        if (i == 8)
                        {
                            month = hexIn;
                            continue;
                        }

                        if (i == 9)
                        {
                            day = hexIn;
                            continue;
                        }

                        if (i == 10)
                        {
                            hour = hexIn;
                            continue;
                        }

                        if (i == 11)
                        {
                            min = hexIn;
                            continue;
                        }

                        if (i == 12)
                        {
                            sec = hexIn;
                            break;
                        }
                    }

                    year = Convert.ToInt32(yearHex, 16);

                    return new DateTime(year, month, day);
                }
            }
            catch(Exception ex)
            {
                Log.Instance.Error(ex, $"Failed to detect Media created date for: {filePath}.");
                return new DateTime();
            }
        }

        public override async Task CopyFileToDetinationDriveAsync(string modFilePath, string destinationDrive, CancellationToken cancelToken)
        {
            var moiFilePath = string.Concat(Path.GetDirectoryName(modFilePath), Path.GetFileNameWithoutExtension(modFilePath) + MOI_EXT);
            FileInfo moiFile = new FileInfo(moiFilePath);

            DateTime dateTaken;
            if(!moiFile.Exists)
            {
                Log.Instance.Warn($"MOD file without MOI: {modFilePath}");
                dateTaken = DateTime.Now;
            }
            else
            {
                dateTaken = GetOriginDateCreation(moiFile.FullName);
            }

            var destinationFolderPath = GetDestinationFolderPath(modFilePath, destinationDrive, dateTaken);
            var destModFilePath = GetDestinationFilePath(modFilePath, destinationFolderPath, dateTaken);

            if (destModFilePath != null)
            {
                await CopyFileAsync(modFilePath, destModFilePath, cancelToken);

                if (moiFile.Exists)
                {
                    var destMoiFilePath = destModFilePath.Replace(MOD_EXT, MOI_EXT);
                    await CopyFileAsync(moiFilePath, destMoiFilePath, cancelToken);
                }
            }
        }
    }
}