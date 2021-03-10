using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DCL
{
    public sealed partial class Mocked
    {
        //TODO(Brian): Use mocking library to replace this mock
        public class File : IFile
        {
            private static Logger logger = new Logger("Mocked.File") {verboseEnabled = false};
            public Dictionary<string, string> mockedFiles = new Dictionary<string, string>();

            public void Delete(string path)
            {
                logger.Verbose($"Deleting {path}");
                if (Exists(path))
                    mockedFiles.Remove(path);
            }

            public bool Exists(string path)
            {
                bool result = mockedFiles.ContainsKey(path);
                logger.Verbose($"Exists? ({path}) == {result}");
                return mockedFiles.ContainsKey(path);
            }

            public void Copy(string srcPath, string dstPath)
            {
                if (!Exists(srcPath))
                    throw new FileNotFoundException($"Not found! ({srcPath})", srcPath);

                logger.Verbose($"Copy from {srcPath} to {dstPath}");
                mockedFiles.Add(dstPath, mockedFiles[srcPath]);
            }

            public void Move(string srcPath, string dstPath)
            {
                if (!Exists(srcPath))
                    throw new FileNotFoundException($"Not found! ({srcPath})", srcPath);

                logger.Verbose($"Move from {srcPath} to {dstPath}");
                Copy(srcPath, dstPath);
                Delete(srcPath);
            }

            public string ReadAllText(string path)
            {
                if (!Exists(path))
                    throw new FileNotFoundException($"Not found! ({path})", path);

                logger.Verbose($"Read all text from {path} = {mockedFiles[path]}");
                return mockedFiles[path];
            }

            public void WriteAllText(string path, string text)
            {
                if (string.IsNullOrEmpty(text))
                    throw new Exception("file contents empty!");

                logger.Verbose($"WriteAllText to {path} = {text}");

                if (!mockedFiles.ContainsKey(path))
                    mockedFiles.Add(path, text);
                else
                    mockedFiles[path] = text;
            }

            public void WriteAllBytes(string path, byte[] bytes)
            {
                if (string.IsNullOrEmpty(path))
                    throw new Exception("Path empty!");

                if (bytes == null)
                    throw new Exception("bytes are null!");

                string stringBytes = System.Text.Encoding.UTF8.GetString(bytes);
                logger.Verbose($"WriteAllText to {path} = {stringBytes}");

                if (!mockedFiles.ContainsKey(path))
                    mockedFiles.Add(path, stringBytes);
                else
                    mockedFiles[path] = stringBytes;
            }

            public Stream OpenRead(string path)
            {
                if (!Exists(path))
                    throw new FileNotFoundException("Not found!", path);

                logger.Verbose($"OpenRead path = {path}");
                return GenerateStreamFromString(mockedFiles[path]);
            }

            private static Stream GenerateStreamFromString(string s)
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(s);
                writer.Flush();
                stream.Position = 0;
                return stream;
            }
        }
    }
}