﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Image.Engine
{
    public class FolderProcessor
    {
        public static async Task ProcessFolder(string sourceFolder, string destinationDrive, Action<string, int> processMonitorAction, CancellationToken cancelToken)
        {
            try
            {
                long progress = 0;
                var files = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories).Select(n=> new FileInfo(n));
                long length = files.Sum(f => f.Length);
                
                foreach(var file in files)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return;
                    }
                    int percentComplete = (int)Math.Round((double)(100 * progress) / length);

                    processMonitorAction($"Copying: {file.FullName}", percentComplete);

                    IFileProcessor processor = Builder.Instance.GetFileProcessor(file.FullName);

                    if(processor != null)
                    {
                        await processor.CopyFileToDetinationDriveAsync(file.FullName, destinationDrive, cancelToken);
                    }

                    progress += file.Length;
                }

                processMonitorAction("Ready", 100);
            }
            catch(TaskCanceledException ex)
            {
                processMonitorAction("Operation canceled", 100);
                Log.Instance.Error(ex);
            }
            catch(Exception ex)
            {
                processMonitorAction("Unhandled Exception", 100);
                //TODO handle no permissions exception
                Log.Instance.Error(ex);
            }
        }
    }
}
