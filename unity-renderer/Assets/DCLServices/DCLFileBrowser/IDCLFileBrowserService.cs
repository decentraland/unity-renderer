using Cysharp.Threading.Tasks;
using DCL;

namespace DCLServices.DCLFileBrowser
{
    public interface IDCLFileBrowserService : IService
    {
        string OpenSingleFile(string title, string directory, string defaultName, params ExtensionFilter[] extensions);
        void SaveFile(string title, string directory, string defaultName, byte[] content, params ExtensionFilter[] extensions);
        UniTask SaveFileAsync(string title, string directory, string defaultName, byte[] content, params ExtensionFilter[] extensions);
    }
}

