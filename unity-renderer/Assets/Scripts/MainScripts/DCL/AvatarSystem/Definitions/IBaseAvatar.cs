using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AvatarSystem
{
    public interface IBaseAvatar
    {
        SkinnedMeshRenderer meshRenderer { get; }
        IBaseAvatarRevealer avatarRevealer { get; set; }

        void Initialize(bool withParticles);
        SkinnedMeshRenderer GetMainRenderer();
        GameObject GetArmatureContainer();
        UniTask FadeOut(MeshRenderer targetRenderer, bool withTransition, CancellationToken cancellationToken);

        void CancelTransition();
    }
}
