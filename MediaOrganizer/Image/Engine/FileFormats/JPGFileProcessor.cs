using System;
using System.IO;

namespace Image.Engine
{
    public class JPGFileProcessor : FileProcessor
    {
        private const string OriginCreationDate = "Date taken";
        private const string OriginDateModified = "Date modified";

        protected override DateTime GetOriginDateCreation(string filePath)
        {
            try
            {
                var dateCreated = base.GetOriginDateCreation(filePath);
                if (dateCreated != DateTime.MinValue)
                {
                    return dateCreated;
                }

                var dateString = MetadataExtractor.Instance.GetFolderDetails(Path.GetDirectoryName(filePath)).GetFileDetail(filePath, OriginCreationDate);

                if (!DateTime.TryParse(dateString, out var dateValue))
                {
                    Log.Instance.Warn($"No 'Date taken' date detected for: {filePath}");

                    /*
                    dateString = MetadataExtractor.Instance.GetFolderDetails(Path.GetDirectoryName(filePath)).GetFileDetail(filePath, OriginDateModified);
                    if (!DateTime.TryParse(dateString, out dateValue))
                    {
                        Log.Instance.Warn($"No 'Date modified' date detected for: {filePath}");
                    }
                    */
                }

                return dateValue;
            }
            catch(Exception ex)
            {
                Log.Instance.Error(ex, $"Failed to detect 'Date taken' date for: {filePath}.");
                return new DateTime();
            }
        }
    }
}
