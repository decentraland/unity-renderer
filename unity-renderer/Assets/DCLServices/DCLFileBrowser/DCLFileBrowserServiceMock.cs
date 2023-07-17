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
            return;
            #endif
            throw new NotImplementedException("Using mock implementation of DCLFileBrowserService in production");
        }

        public UniTask SaveFileAsync(string title, string directory, string defaultName, byte[] content, ExtensionFilter[] extensions)
        {
            #if UNITY_EDITOR
            return new UniTask();
            #endif
            throw new NotImplementedException("Using mock implementation of DCLFileBrowserService in production");
        }

        public void Dispose() { }
    }
}
