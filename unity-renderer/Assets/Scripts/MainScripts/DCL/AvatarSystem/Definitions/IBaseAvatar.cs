using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        UniTask FadeOut(MeshRenderer targetRenderer, bool withTransition, CancellationToken cancellationToken);

        void CancelTransition();
    }
}
