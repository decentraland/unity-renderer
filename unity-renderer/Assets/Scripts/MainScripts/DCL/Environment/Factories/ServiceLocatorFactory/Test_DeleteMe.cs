using Cysharp.Threading.Tasks;
using DCLServices.DCLFileBrowser;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class Test_DeleteMe : MonoBehaviour
    {
        public Button openFileButton;
        public Button saveFileButton;
        public Button saveFileAsyncButton;

        private void Awake()
        {
            openFileButton.onClick.AddListener(OpenFile);
            saveFileButton.onClick.AddListener(SaveFile);
            saveFileAsyncButton.onClick.AddListener(SaveFileAsync);
        }

        public void OpenFile()
        {
            var result = Environment.i.serviceLocator.Get<IDCLFileBrowserService>().OpenSingleFile("Open File", "", "test", new ExtensionFilter("Text", "txt"));
            Log(result);

        }

        public void SaveFile()
        {
            var result = Environment.i.serviceLocator.Get<IDCLFileBrowserService>().SaveFile("Save File", "", "test", new ExtensionFilter("Text", "txt"));
            Log(result);
        }

        public void SaveFileAsync()
        {
            SaveFileAsyncTask().Forget();
        }

        public async UniTask SaveFileAsyncTask()
        {
            var result = await Environment.i.serviceLocator.Get<IDCLFileBrowserService>().SaveFileAsync("Save File", "", "test", new ExtensionFilter("Text", "txt"));
            Log(result);
        }

        private void Log(string s)
        {
            bool oldLog = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = true;
            Debug.Log($"Test: {s}");
            Debug.unityLogger.logEnabled = oldLog;
        }
    }
}
