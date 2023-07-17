using Cysharp.Threading.Tasks;
using System;

namespace DCLServices.DCLFileBrowser
{
    public class DCLFileBrowserServiceWebGL : IDCLFileBrowserService
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public string OpenSingleFile(string title, string directory, string defaultName, params ExtensionFilter[] extensions) =>
            throw new NotImplementedException();

        public void SaveFile(string title, string directory, string defaultName, byte[] content, params ExtensionFilter[] extensions)
        {
            if (!WebGLFileSaver.IsSavingSupported())
                throw new Exception("File saving is not supported in the browser");
            WebGLFileSaver.SaveFile(content, defaultName);
        }

        public UniTask SaveFileAsync(string title, string directory, string defaultName, byte[] content, params ExtensionFilter[] extensions) =>
            throw new NotImplementedException();
    }
}
