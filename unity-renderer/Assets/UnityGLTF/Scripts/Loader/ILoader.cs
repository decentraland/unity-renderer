using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

public delegate bool AssetIdConverter(string uri, out string id);

namespace UnityGLTF.Loader
{
    public interface ILoader
    {
        UniTask LoadStream(string relativeFilePath, CancellationToken token);

        void LoadStreamSync(string jsonFilePath);

        Stream LoadedStream { get; }

        bool HasSyncLoadMethod { get; }

        AssetIdConverter assetIdConverter { get; }
    }
}