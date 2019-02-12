using System;
using System.IO;

namespace Image.Engine
{
    public class MP4FileProcessor : FileProcessor
    {
        private const string OriginCreationDate = "Media created";

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
                    Log.Instance.Warn($"No 'Media created' date detected for: {filePath}");
                }

                return dateValue;
            }
            catch (Exception ex)
            {
                Log.Instance.Error(ex, $"Failed to detect 'Media created' date for: {filePath}.");
                return new DateTime();
            }
        }
    }
}
