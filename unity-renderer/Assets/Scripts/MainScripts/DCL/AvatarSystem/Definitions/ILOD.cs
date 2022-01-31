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
        /// <summary>
        /// Set color ignoring the alpha
        /// </summary>
        /// <param name="color"></param>
        void SetImpostorTint(Color color);
    }
}