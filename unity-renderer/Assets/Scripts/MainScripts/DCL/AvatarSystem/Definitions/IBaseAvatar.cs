using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public interface IBaseAvatar
    {

        SkinnedMeshRenderer meshRenderer { get; }

        void Initialize();
        SkinnedMeshRenderer GetMainRenderer();
        GameObject GetArmatureContainer();
        void FadeOut(MeshRenderer targetRenderer);
    }
}
