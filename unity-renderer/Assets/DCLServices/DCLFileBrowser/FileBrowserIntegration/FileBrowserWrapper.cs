using Crosstales.FB;
using Cysharp.Threading.Tasks;

namespace DCLServices.DCLFileBrowser.FileBrowserIntegration
{
    public class FileBrowserWrapper : IDCLFileBrowserService
    {
        public void Initialize()
        {
#if UNITY_WEBGL
            FileBrowser.Instance.CustomWrapper = FileBrowser.Instance.gameObject.AddComponent<Crosstales.FB.WebGL.FileBrowserWebGL>();
#endif
        }

        public string OpenSingleFile(string title, string directory, string defaultName, params ExtensionFilter[] extensions) =>
            FileBrowser.Instance.OpenSingleFile(title, directory, defaultName, ConvertExtensionFilters(extensions));

        public string SaveFile(string title, string directory, string defaultName, params ExtensionFilter[] extensions) =>
            FileBrowser.Instance.SaveFile(title, directory, defaultName, ConvertExtensionFilters(extensions));

        public UniTask<string> SaveFileAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions)
        {
            UniTaskCompletionSource<string> completionSource = new UniTaskCompletionSource<string>();
            FileBrowser.Instance.SaveFileAsync(path => completionSource.TrySetResult(path), title, directory, defaultName, ConvertExtensionFilters(extensions));
            return completionSource.Task;
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
