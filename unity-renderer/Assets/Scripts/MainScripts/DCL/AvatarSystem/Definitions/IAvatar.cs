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
        }

        Status status { get; }
        Vector3 extents { get; }
        int lodLevel { get; }

        UniTask Load(List<string> wearablesIds, List<string> emotesIds, AvatarSettings settings, CancellationToken ct = default);
        void AddVisibilityConstraint(string key);
        void RemoveVisibilityConstrain(string key);
        void PlayEmote(string emoteId, long timestamps);
        void SetLODLevel(int lodIndex);
        void SetAnimationThrottling(int framesBetweenUpdate);
        void SetImpostorTexture(Texture2D impostorTexture);
        void SetImpostorTint(Color color);
        Transform[] GetBones();
    }

}