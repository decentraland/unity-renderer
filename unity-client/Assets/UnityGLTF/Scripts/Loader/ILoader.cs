using System.Collections;
using System.IO;
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
