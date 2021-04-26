using UnityEngine.Networking;

namespace DCL
{
    public class MapChunk_Mock : MapChunk
    {
        public override WebRequestAsyncOperation LoadChunkImage()
        {
            isLoadingOrLoaded = true;

            return new WebRequestAsyncOperation(null);
        }
    }
}