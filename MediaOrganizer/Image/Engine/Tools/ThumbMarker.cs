using System;
using System.IO;
using System.Linq;

namespace Image.Engine
{
    public class ThumbMarker
    {
        private const string _thumbFileName = "thumb.al";

        public static void PutThumbMarker(string folderPath, bool inDeep)
        {
            DirectoryInfo folder = new DirectoryInfo(folderPath);
            if(!folder.Exists)
            {
                return;
            }

            if(!folder.GetFiles(_thumbFileName).Any())
            {
                PutMarker(folderPath);
            }

            if(inDeep)
            {
                foreach(var f in folder.GetDirectories())
                {
                    PutThumbMarker(f.FullName, inDeep);
                }
            }
        }

        public static bool HasThumpMarker(string folderPath)
        {
            DirectoryInfo folder = new DirectoryInfo(folderPath);
            if (!folder.Exists)
            {
                return false;
            }
            try
            {
                return folder.GetFiles(_thumbFileName).Any();
            }
            catch(UnauthorizedAccessException)
            {
                return false;
            }
        }

        public static void DelThumbMarker(string folderPath, bool inDeep)
        {
            DirectoryInfo folder = new DirectoryInfo(folderPath);
            if (!folder.Exists)
            {
                return;
            }

            if (folder.GetFiles(_thumbFileName).Any())
            {
                DelMarker(folderPath);
            }

            if (inDeep)
            {
                foreach (var f in folder.GetDirectories())
                {
                    DelThumbMarker(f.FullName, inDeep);
                }
            }
        }

        private static void PutMarker(string folderPath)
        {
            var fileName = Path.Combine(folderPath, _thumbFileName);
            using (File.Create(fileName))
            {

            }
        }

        private static void DelMarker(string folderPath)
        {
            var fileName = Path.Combine(folderPath, _thumbFileName);

            try
            {
                File.Delete(fileName);
            }
            catch(Exception ex)
            {
                Log.Instance.Error(ex);
            }
        }
    }
}
