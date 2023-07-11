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

        public string SaveFile(string title, string directory, string defaultName, params ExtensionFilter[] extensions)
        {
            #if UNITY_EDITOR
            return $"{Application.persistentDataPath}/{directory}/{defaultName}";
            #endif
            throw new NotImplementedException("Using mock implementation of DCLFileBrowserService in production");
        }

        public async UniTask<string> SaveFileAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions)
        {
            #if UNITY_EDITOR
            return $"{Application.persistentDataPath}/{directory}/{defaultName}";
            #endif
            throw new NotImplementedException("Using mock implementation of DCLFileBrowserService in production");
        }

        public void Dispose() { }
    }
}
