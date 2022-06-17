using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public interface IBaseAvatar
    {
        SkinnedMeshRenderer meshRenderer { get; }
        IBaseAvatarRevealer avatarRevealer { get; set; }

        void Initialize();
        SkinnedMeshRenderer GetMainRenderer();
        GameObject GetArmatureContainer();
        void FadeOut(MeshRenderer targetRenderer, bool playParticles);
    }
}
