using UnityEngine;

namespace AvatarSystem
{
    public interface IBodyshapeLoader : IWearableLoader
    {
        WearableItem eyes { get; }
        WearableItem eyebrows { get; }
        WearableItem mouth { get; }

        SkinnedMeshRenderer headRenderer { get; }
        SkinnedMeshRenderer feetRenderer { get; }
        SkinnedMeshRenderer upperBodyRenderer { get; }
        SkinnedMeshRenderer lowerBodyRenderer { get; }
    }
}