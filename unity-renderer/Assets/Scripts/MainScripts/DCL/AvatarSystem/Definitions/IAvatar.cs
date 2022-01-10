using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AvatarSystem
{
    public interface IAvatar : IDisposable
    {
        public enum Status
        {
            Idle,
            Loaded,
            Failed
        }

        Status status { get; }
        Vector3 extents { get; }

        UniTask Load(List<string> wearablesIds, AvatarSettings settings, CancellationToken ct = default);
        void SetVisibility(bool visible);
        void SetExpression(string expressionId, long timestamps);
        void SetLODLevel(int lodIndex);
        void SetAnimationThrottling(int framesBetweenUpdate);
        void SetImpostorTexture(Texture2D impostorTexture);
        void SetImpostorTint(Color color);
        Transform[] GetBones();
    }
}