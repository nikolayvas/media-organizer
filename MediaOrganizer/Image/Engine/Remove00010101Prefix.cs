using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Image.Engine
{
    public class Remove00010101Prefix
    {
        private const string _prefix = "00010101_";
        public static void Do(string dirPath, Action<string, int> processMonitorAction, CancellationToken cancelToken)
        {
            try
            {
                var dir = new DirectoryInfo(dirPath);
                if (dir.Exists)
                {
                    Do(dir, processMonitorAction, cancelToken);
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Error(ex, "Remove00010101Prefix operation failed!");
                processMonitorAction("Remove00010101Prefix operation failed!", 100);
            }
        }

        private static void Do(DirectoryInfo dir, Action<string, int> processMonitorAction, CancellationToken cancelToken)
        {
            processMonitorAction($"Remove 00010101 files prefix for folder {dir.FullName} started!", 0);
            Dictionary<string, List<FileInfo>> dict = new Dictionary<string, List<FileInfo>>();

            foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories))
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                if(Path.GetFileName(file.FullName).StartsWith(_prefix))
                {
                    File.Move(file.FullName, file.FullName.Replace(_prefix, ""));
                }
            }

            processMonitorAction("Remove00010101Prefix completed!", 100);
        }
    }
}
