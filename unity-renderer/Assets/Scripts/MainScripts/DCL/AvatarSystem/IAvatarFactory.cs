using DCL;
using System;
using UnityEngine;

namespace AvatarSystem
{
    public interface IAvatarFactory : IService
    {
        Avatar CreateAvatar(
            GameObject avatarContainer,
            IAnimator animator,
            ILOD lod,
            IVisibility visibility);

        AvatarWithHologram CreateAvatarWithHologram(
            GameObject avatarContainer,
            Transform avatarRevealContainer,
            GameObject armatureContainer,
            IAnimator animator,
            ILOD lod,
            IVisibility visibility
        );

        void IDisposable.Dispose() { }

        void IService.Initialize() { }
    }
}
