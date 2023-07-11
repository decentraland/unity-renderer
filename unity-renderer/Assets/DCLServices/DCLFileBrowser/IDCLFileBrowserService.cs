using Cysharp.Threading.Tasks;
using DCL;

namespace DCLServices.DCLFileBrowser
{
    public interface IDCLFileBrowserService : IService
    {
        string OpenSingleFile(string title, string directory, string defaultName, params ExtensionFilter[] extensions);
        string SaveFile(string title, string directory, string defaultName, params ExtensionFilter[] extensions);
        UniTask<string> SaveFileAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions);
    }
}

