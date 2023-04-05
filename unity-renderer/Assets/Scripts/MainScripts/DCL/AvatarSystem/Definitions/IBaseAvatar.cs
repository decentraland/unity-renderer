using System.Threading;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace AvatarSystem
{
    public interface IBaseAvatar : IDisposable
    {
        SkinnedMeshRenderer SkinnedMeshRenderer { get; }
        GameObject ArmatureContainer { get; }

        UniTask FadeGhost(CancellationToken cancellationToken = default);

        UniTask Reveal(Renderer targetRenderer, float avatarHeight, float completionHeight, CancellationToken cancellationToken = default);

        void RevealInstantly(Renderer targetRenderer, float avatarHeight);
    }
}
