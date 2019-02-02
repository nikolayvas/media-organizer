using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Image.Engine
{
    public sealed class ImageFactoryBuilder
    {
        private Dictionary<string, Type> _registeredFileProcessors = new Dictionary<string, Type>();
        private ConcurrentDictionary<string, IFileProcessor> _fileProcessors = new ConcurrentDictionary<string, IFileProcessor>();

        private static ImageFactoryBuilder instance = null;

        private ImageFactoryBuilder()
        {
            _registeredFileProcessors.Add(".jpg", typeof(JPGFileProcessor));
            _registeredFileProcessors.Add(".mod", typeof(JVCMovieFileProcessor));
            _registeredFileProcessors.Add(".mov", typeof(MP4FileProcessor));
            _registeredFileProcessors.Add(".mp4", typeof(MP4FileProcessor));
            _registeredFileProcessors.Add(".3gp", typeof(MP4FileProcessor));
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
            var ext = Path.GetExtension(sourceFilePath).ToLower();
            return _fileProcessors.GetOrAdd(ext, x => 
                {
                    if (_registeredFileProcessors.TryGetValue(ext, out Type processorType))
                    {
                        IFileProcessor instance = (IFileProcessor)Activator.CreateInstance(processorType);
                        return instance;
                    }

                    Log.Instance.Info($"Unsupported file format: {sourceFilePath}");
                    return null;
                });
        }
    }
}
