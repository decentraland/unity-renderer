using System;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public interface ILOD : IDisposable
    {
        int lodIndex { get; }
        void Bind(Renderer combinedAvatar);
        void SetLodIndex(int lodIndex, bool inmediate = false);
        void SetImpostorTexture(Texture2D texture);
        void SetImpostorTint(Color color);
    }
}