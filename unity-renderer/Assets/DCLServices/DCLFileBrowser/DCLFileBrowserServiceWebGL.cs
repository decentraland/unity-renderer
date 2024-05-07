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
            SaveFile(defaultName, content, extensions);
        }

        public UniTask SaveFileAsync(string title, string directory, string defaultName, byte[] content, params ExtensionFilter[] extensions)
        {
            SaveFile(defaultName, content, extensions);
            return UniTask.CompletedTask;
        }

        private void SaveFile(string fileName, byte[] content, ExtensionFilter[] extensionsFilter)
        {
            if (extensionsFilter.Length > 0 && extensionsFilter[0].Extensions.Length > 0  && !FileNameContainsAnyExtension(fileName, extensionsFilter))
            {
                fileName += $".{extensionsFilter[0].Extensions[0]}";
            }

            if (!WebGLFileSaver.IsSavingSupported())
                throw new Exception("File saving is not supported in the browser");
            WebGLFileSaver.SaveFile(content, fileName);
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
