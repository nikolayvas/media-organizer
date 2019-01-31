using System;
using System.Collections.Generic;
using System.IO;

namespace Image.Engine
{
    public sealed class Builder
    {
        private Dictionary<string, Type> _registeredFileProcessors = new Dictionary<string, Type>();

        private static Builder instance = null;

        private Builder()
        {
            _registeredFileProcessors.Add(".jpg", typeof(JPGFileProcessor));
            _registeredFileProcessors.Add(".mov", typeof(JVCMovieFileProcessor));
            _registeredFileProcessors.Add(".mp4", typeof(MP4FileProcessor));
        }

        public static Builder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Builder();
                }
                return instance;
            }
        }

        public IFileProcessor GetFileProcessor(string sourceFilePath)
        {
            try
            {
                var ext = Path.GetExtension(sourceFilePath).ToLower();

                if(_registeredFileProcessors.TryGetValue(ext, out Type processorType))
                {
                    IFileProcessor instance = (IFileProcessor)Activator.CreateInstance(processorType);
                    return instance;
                }
            }
            catch(Exception ex)
            {
                Log.Instance.Error(ex);
            }

            Log.Instance.Info($"Unsupported file format: {sourceFilePath}");
            return null;
        }
    }
}
