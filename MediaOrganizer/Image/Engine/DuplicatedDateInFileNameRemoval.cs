using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Image.Engine
{
    public class DuplicatedDateInFileNameRemoval
    {
        private static readonly string _dateFormat = "yyyyMMdd";
        private static readonly string _dateFormat2 = "yyyy-MM-dd";

        public static void UpdateNames(string dirPath, Action<string, int> processMonitorAction, CancellationToken cancelToken)
        {
            var dir = new DirectoryInfo(dirPath);
            if (dir.Exists)
            {
                UpdateNames(dir, processMonitorAction, cancelToken);
            }
        }

        private static void UpdateNames(DirectoryInfo dir, Action<string, int> processMonitorAction, CancellationToken cancelToken)
        {
            processMonitorAction("Update file names started!", 0);

            var files = dir.GetFiles("*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    DoAction(file);
                }
                catch(Exception ex)
                {
                    Log.Instance.Error(ex, $"Failed to process file {file.FullName}");
                }
            }

            processMonitorAction("Update file names completed!", 100);
        }

        private static void DoAction(FileInfo file)
        {
            var fileName = Path.GetFileNameWithoutExtension(file.FullName);

            //yyyyMMdd_yyyy-MM-dd xxxxxxx
            if (fileName[8] == '_' && fileName[13] == '-' && fileName[16] == '-' && fileName[19] == ' ')
            {
                var date1 = fileName.Substring(0, 8);
                var date2 = fileName.Substring(9, 10);

                if (DateTime.TryParseExact(date1, _dateFormat, null, DateTimeStyles.None, out var dateCreated1) &&
                    DateTime.TryParseExact(date2, _dateFormat2, null, DateTimeStyles.None, out var dateCreated2))
                {
                    if(dateCreated1 == dateCreated2)
                    {
                        var newName = Path.Combine(file.DirectoryName, fileName.Substring(9) + file.Extension);
                        Log.Instance.Warn($"File {file.FullName} was renamed to {newName}!");

                        File.Move(file.FullName, newName);
                    }
                }
            }
        }
    }
}
