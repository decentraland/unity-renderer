using System;
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

        GameObject combinedAvatar { get; }
        GameObject[] facialFeatures { get; }
        Status status { get; }
        UniTaskVoid Load(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, WearableItem[] wearables, AvatarSettings settings);
    }
}