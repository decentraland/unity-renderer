using System.Collections;
using System.IO;
using System.Threading.Tasks;

namespace UnityGLTF.Loader
{
    public interface ILoaderAsync
    {
        Task LoadStream(string relativeFilePath);

        void LoadStreamSync(string jsonFilePath);

        Stream LoadedStream { get; }

        bool HasSyncLoadMethod { get; }

        AssetIdConverter assetIdConverter { get; }
    }
}