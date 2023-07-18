using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public interface IBodyshapeLoader : IWearableLoader
    {
        WearableItem eyes { get; }
        WearableItem eyebrows { get; }
        WearableItem mouth { get; }

        SkinnedMeshRenderer eyesRenderer { get; }
        SkinnedMeshRenderer eyebrowsRenderer { get; }
        SkinnedMeshRenderer mouthRenderer { get; }
        SkinnedMeshRenderer headRenderer { get; }
        SkinnedMeshRenderer feetRenderer { get; }
        SkinnedMeshRenderer handsRenderer { get; }
        SkinnedMeshRenderer upperBodyRenderer { get; }
        SkinnedMeshRenderer lowerBodyRenderer { get; }
        IList<SkinnedMeshRenderer> extraRenderers { get; }

        bool IsValid(BodyWearables bodyWearables);
        void DisableFacialRenderers();
    }
}
