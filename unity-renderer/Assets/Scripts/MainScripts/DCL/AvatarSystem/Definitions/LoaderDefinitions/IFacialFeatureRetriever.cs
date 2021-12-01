using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AvatarSystem
{
    public interface IFacialFeatureRetriever : IDisposable
    {
        UniTask<(Texture main, Texture mask)> Retrieve(string mainTextureUrl, string maskTextureUrl);
    }

    public interface IAvatarCombiner { }
}