using System;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DCL;
using UnityEngine;
using UnityEngine.UI;

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

    public struct AvatarSettings
    {
        public Color hairColor;
        public Color skinColor;
        public Color eyeColor;
        public string bodyshapeId;
    }

    public interface IBodyshapeLoader : IWearableLoader
    {
        WearableItem eyes { get; }
        WearableItem eyebrows { get; }
        WearableItem mouth { get; }

        SkinnedMeshRenderer headRenderer { get; }
        SkinnedMeshRenderer feetRenderer { get; }
        SkinnedMeshRenderer upperBodyRenderer { get; }
        SkinnedMeshRenderer lowerBodyRenderer { get; }
    }

    public interface IWearableLoader : IDisposable
    {
        public enum Status
        {
            Idle,
            Succeeded,
            Defaulted,
            Failed
        }

        WearableItem wearable { get; }
        Rendereable rendereable { get; }
        Status status { get; }
        UniTask Load(GameObject container, AvatarSettings avatarSettings);
    }

    public interface IFacialFeatureRetriever : IDisposable
    {
        UniTask<(Texture main, Texture mask)> Retrieve(string mainTextureUrl, string maskTextureUrl);
    }

    public interface IWearableRetriever : IDisposable
    {
        Rendereable rendereable { get; }
        UniTask<Rendereable> Retrieve(GameObject container, ContentProvider contentProvider, string baseUrl, string mainFile);
    }

    public interface IWearableLoaderFactory
    {
        IWearableLoader GetWearableLoader(WearableItem item);
        IBodyshapeLoader GetBodyshapeLoader(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth);
    }

    public interface IRetrieverFactory
    {
        IWearableRetriever GetWearableRetriever();
        IFacialFeatureRetriever GetFacialFeatureRetriever();
    }
}