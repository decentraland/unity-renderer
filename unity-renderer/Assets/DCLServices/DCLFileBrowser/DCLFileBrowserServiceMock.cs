using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace DCLServices.DCLFileBrowser
{
    public class DCLFileBrowserServiceMock : IDCLFileBrowserService
    {
        public void Initialize() { }

        public string OpenSingleFile(string title, string directory, string defaultName, params ExtensionFilter[] extensions)
        {
            #if UNITY_EDITOR
            return $"{Application.persistentDataPath}/{directory}/{defaultName}";
            #endif
            throw new NotImplementedException("Using mock implementation of DCLFileBrowserService in production");
        }

        public void SaveFile(string title, string directory, string defaultName, byte[] content, params ExtensionFilter[] extensions)
        {
            #if UNITY_EDITOR
            defaultName = GetNameWithExtension(defaultName, extensions);
            var path = $"{Application.persistentDataPath}/{defaultName}";
            System.IO.File.WriteAllBytes(path, content);
            Debug.Log($"Exported at {path}");
            return;
            #endif
            throw new NotImplementedException("Using mock implementation of DCLFileBrowserService in production");
        }

        public UniTask SaveFileAsync(string title, string directory, string defaultName, byte[] content, ExtensionFilter[] extensions)
        {
            #if UNITY_EDITOR
            defaultName = GetNameWithExtension(defaultName, extensions);
            var path = $"{Application.persistentDataPath}/{defaultName}";
            System.IO.File.WriteAllBytes(path, content);
            Debug.Log($"Exported at {path}");
            return UniTask.CompletedTask;
            #endif
            throw new NotImplementedException("Using mock implementation of DCLFileBrowserService in production");
        }

        public void Dispose() { }

        private string GetNameWithExtension(string fileName, ExtensionFilter[] extensionsFilter)
        {
            if (extensionsFilter.Length > 0 && extensionsFilter[0].Extensions.Length > 0  && !FileNameContainsAnyExtension(fileName, extensionsFilter))
            {
                fileName += $".{extensionsFilter[0].Extensions[0]}";
            }

            return fileName;
        }

        private bool FileNameContainsAnyExtension(string fileName, ExtensionFilter[] extensionsFilter)
        {
            foreach (ExtensionFilter extensionFilter in extensionsFilter)
            {
                foreach (string extension in extensionFilter.Extensions)
                {
                    if (fileName.EndsWith(extension))
                        return true;
                }
            }
            return false;
        }
    }
}
