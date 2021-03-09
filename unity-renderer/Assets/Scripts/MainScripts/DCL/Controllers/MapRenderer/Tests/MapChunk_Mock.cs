using System.Collections;

namespace DCL
{
    public class MapChunk_Mock : MapChunk
    {
        public override IEnumerator LoadChunkImage()
        {
            if (isLoadingOrLoaded)
                yield break;

            isLoadingOrLoaded = true;
        }
    }
}
