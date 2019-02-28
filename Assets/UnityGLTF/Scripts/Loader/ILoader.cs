using System.Collections;
using System.IO;
using GLTF;
using GLTF.Schema;
namespace UnityGLTF.Loader
{
    public interface ILoader
    {
        IEnumerator LoadStream(string relativeFilePath);

        void LoadStreamSync(string jsonFilePath);

        Stream LoadedStream { get; }

        bool HasSyncLoadMethod { get; }
    }
}
