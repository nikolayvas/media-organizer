using System.Collections.Concurrent;

namespace Image.Engine
{
    public sealed class MetadataExtractor
    {
        private readonly ConcurrentDictionary<string, FolderDetail> _folderDetails = new ConcurrentDictionary<string, FolderDetail>(); 

        private static MetadataExtractor instance = null;

        private MetadataExtractor()
        {
        }

        public static MetadataExtractor Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MetadataExtractor();
                }
                return instance;
            }
        }

        public FolderDetail GetFolderDetails(string folderPath)
        {
            return _folderDetails.GetOrAdd(folderPath, new FolderDetail(folderPath));
        }
    }
}
