using System;
using System.Collections.Generic;
using System.IO;

namespace Image.Engine
{
    public sealed class ImageFactoryBuilder
    {
        private Dictionary<string, Type> _registeredFileProcessors = new Dictionary<string, Type>();

        private static ImageFactoryBuilder instance = null;

        private ImageFactoryBuilder()
        {
            _registeredFileProcessors.Add(".jpg", typeof(JPGFileProcessor));
            _registeredFileProcessors.Add(".mod", typeof(JVCMovieFileProcessor));
            _registeredFileProcessors.Add(".mov", typeof(MP4FileProcessor));
            _registeredFileProcessors.Add(".mp4", typeof(MP4FileProcessor));
        }

        public static ImageFactoryBuilder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ImageFactoryBuilder();
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
