using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;

namespace Image.Engine
{
    public class DuplicatedFilesRemoval
    {
        public static void RemoveDuplicatedFilesPerFolder(string dirPath, bool inDeep, Action<string, int> processMonitorAction, CancellationToken cancelToken)
        {
            try
            {
                var dir = new DirectoryInfo(dirPath);
                if (dir.Exists)
                {
                    RemoveDuplicatedFilesPerFolder(dir, inDeep, processMonitorAction, cancelToken);
                }
            }
            catch(Exception ex)
            {
                Log.Instance.Error(ex, "Remove duplicated files operation failed!");
                processMonitorAction("Remove duplicated files operation failed!", 100);
            }
        }

        private static void RemoveDuplicatedFilesPerFolder(DirectoryInfo dir, bool inDeep, Action<string, int> processMonitorAction, CancellationToken cancelToken)
        {
            processMonitorAction("Remove duplicated files started!", 0);
            Dictionary<string, List<FileInfo>> dict = new Dictionary<string, List<FileInfo>>();

            foreach(var file in dir.GetFiles())
            {
                if(cancelToken.IsCancellationRequested)
                {
                    return;
                }

                var originName = GetFileOriginName(file);

                if (!dict.TryGetValue(originName, out List<FileInfo> list))
                {
                    dict[originName] = new List<FileInfo>() { file };
                }
                else
                {
                    list.Add(file);
                }
            }

            foreach(var pair in dict.Where(n=>n.Value.Count > 1))
            {
                processMonitorAction($"Merge '{pair.Key}' ...", 100);
                DoAction(pair.Key, pair.Value);
            }

            if(inDeep)
            {
                foreach(var subDir in dir.GetDirectories())
                {
                    RemoveDuplicatedFilesPerFolder(subDir, inDeep, processMonitorAction, cancelToken);
                }
            }

            processMonitorAction("Remove duplicated files completed!", 100);
        }

        private static void DoAction(string fileName, List<FileInfo> files)
        {
            long maxLenght = 0;
            FileInfo maxFile = null;
            foreach(var file in files)
            {
                if(file.Length > maxLenght)
                {
                    maxFile = file;
                    maxLenght = file.Length;
                }
            }

            foreach (var file in files)
            {
                if (file != maxFile)
                {
                    Log.Instance.Warn($"Duplicated file {file.FullName} was deleted ");
                    file.Delete();
                }
            }

            Log.Instance.Warn($"Duplicated file '{maxFile.FullName}' was renamed to '{fileName}'");
            File.Move(maxFile.FullName, fileName);
        }

        private static string GetFileOriginName(FileInfo file)
        {
            string path = Path.GetDirectoryName(file.FullName);
            string name = Path.GetFileNameWithoutExtension(file.FullName);
            string ext = Path.GetExtension(file.FullName);

            if(name.Length > 2 && name[name.Length - 2] == '_')
            {
                return Path.Combine(path, name.Substring(0, name.Length - 2) + ext);
            }
            else
            {
                return file.FullName;
            }
        }

        private IEnumerable<string> GetSuffix()
        {
            var index = 1;
            while(index < 10)
            {
                yield return $"_{index++}";
            }
        }
    }
}
