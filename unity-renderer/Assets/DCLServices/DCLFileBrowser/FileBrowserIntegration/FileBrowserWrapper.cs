using Crosstales.FB;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace DCLServices.DCLFileBrowser.FileBrowserIntegration
{
    [UsedImplicitly]
    public class FileBrowserWrapper : IDCLFileBrowserService
    {
        public void Initialize() { }

        public string OpenSingleFile(string title, string directory, string defaultName, params ExtensionFilter[] extensions) =>
            FileBrowser.Instance.OpenSingleFile(title, directory, defaultName, ConvertExtensionFilters(extensions));

        public void SaveFile(string title, string directory, string defaultName, byte[] content, params ExtensionFilter[] extensions)
        {
            var path = FileBrowser.Instance.SaveFile(title, directory, defaultName, ConvertExtensionFilters(extensions));
            System.IO.File.WriteAllBytes(path, content);
        }

        public async UniTask SaveFileAsync(string title, string directory, string defaultName, byte[] content, ExtensionFilter[] extensions)
        {
            UniTaskCompletionSource<string> completionSource = new UniTaskCompletionSource<string>();
            FileBrowser.Instance.SaveFileAsync(path => completionSource.TrySetResult(path), title, directory, defaultName, ConvertExtensionFilters(extensions));
            var path = await completionSource.Task;
            await System.IO.File.WriteAllBytesAsync(path, content);
        }

        private static Crosstales.FB.ExtensionFilter[] ConvertExtensionFilters(ExtensionFilter[] extensions)
        {
            Crosstales.FB.ExtensionFilter[] crosstalesExtensions = new Crosstales.FB.ExtensionFilter[extensions.Length];

            for (var i = 0; i < extensions.Length; i++)
            {
                var extension = extensions[i];
                crosstalesExtensions[i] = new Crosstales.FB.ExtensionFilter(extension.Name, extension.Extensions);
            }

            return crosstalesExtensions;
        }

        public void Dispose()
        {
            FileBrowser.DeleteInstance();
        }
    }
}
