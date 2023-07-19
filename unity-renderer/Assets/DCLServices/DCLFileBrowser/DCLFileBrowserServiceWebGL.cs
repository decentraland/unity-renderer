using Cysharp.Threading.Tasks;
using System;

namespace DCLServices.DCLFileBrowser
{
    public class DCLFileBrowserServiceWebGL : IDCLFileBrowserService
    {
        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        public string OpenSingleFile(string title, string directory, string defaultName, params ExtensionFilter[] extensions) =>
            throw new Exception("File opening is not supported in the browser");

        public void SaveFile(string title, string directory, string defaultName, byte[] content, params ExtensionFilter[] extensions)
        {
            SaveFile(defaultName, content);
        }

        public UniTask SaveFileAsync(string title, string directory, string defaultName, byte[] content, params ExtensionFilter[] extensions)
        {
            SaveFile(defaultName, content);
            return UniTask.CompletedTask;
        }

        private void SaveFile(string fileName, byte[] content)
        {
            if (!WebGLFileSaver.IsSavingSupported())
                throw new Exception("File saving is not supported in the browser");
            WebGLFileSaver.SaveFile(content, fileName);
        }
    }
}
