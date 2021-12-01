using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AvatarSystem
{
    public interface ILoader : IDisposable
    {
        public enum Status
        {
            Idle,
            Loading,
            Succeeded,
            Failed_Mayor,
            Failed_Minor
        }

        Renderer combinedRenderer { get; }
        Renderer eyesRenderer { get; }
        Renderer eyebrowsRenderer { get; }
        Renderer mouthRenderer { get; }
        Status status { get; }
        UniTask Load(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<WearableItem> wearables, AvatarSettings settings);
    }
}