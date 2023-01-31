using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace DCL
{
    public class MapChunk_Mock : MapChunk
    {
        public override async UniTask<UnityWebRequest> LoadChunkImage()
        {
            isLoadingOrLoaded = true;
            return new UnityWebRequest();
        }
    }
}
