using System;
using System.Collections.Generic;

namespace DCL
{
    public sealed partial class Mocked
    {
        //TODO(Brian): Use mocking library to replace this mock
        public class Directory : IDirectory
        {
            public Dictionary<string, string> mockedDirs = new Dictionary<string, string>();

            public void CreateDirectory(string path)
            {
                if (string.IsNullOrEmpty(path))
                    throw new Exception("file contents empty!");

                mockedDirs.Add(path, path);
            }

            public void InitializeDirectory(string path, bool deleteIfExists) { Delete(path); }

            public void Delete(string path, bool recursive = true)
            {
                if (Exists(path))
                    mockedDirs.Remove(path);
            }

            public bool Exists(string path) { return mockedDirs.ContainsKey(path); }
        }
    }
}