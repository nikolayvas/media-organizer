﻿using Shell32;
using System.Collections.Generic;
using System.IO;
namespace Image.Engine
{
    public class FolderDetail
    {
        private Dictionary<string, int> _headers = new Dictionary<string, int>();
        private readonly char[]  _charactersToRemove = new char[] { (char)8206, (char)8207 };

        private Folder _folder;

        public FolderDetail(string path)
        {
            _folder = new Shell().NameSpace(path);

            for (int i = 0; i < 512; i++)
            {
                string header = _folder.GetDetailsOf(null, i);
                if (!string.IsNullOrEmpty(header))
                {
                    _headers[header] = i;
                }
            }
        }

        public string GetFileDetail(string filePath, string headerName)
        {
            FolderItem folderItem = _folder.ParseName(Path.GetFileName(filePath));

            if (_headers.TryGetValue(headerName, out int index))
            {
                var value = _folder.GetDetailsOf(folderItem, index);

                // Removing the suspect characters
                foreach (char c in _charactersToRemove)
                {
                    value = value.Replace((c).ToString(), "").Trim();
                }

                return value;
            }

            return null;
        }
    }
}
