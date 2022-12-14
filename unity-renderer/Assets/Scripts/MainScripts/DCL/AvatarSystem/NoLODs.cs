using DCL;
using UnityEngine;

namespace AvatarSystem
{
    public class NoLODs : Singleton<NoLODs>, ILOD
    {
        public int lodIndex { get; }

        public void Bind(Renderer combinedAvatar) { }
        public void SetLodIndex(int lodIndex, bool inmediate = false) { }
        public void SetImpostorTexture(Texture2D texture) { }
        public void SetImpostorTint(Color color) { }

        public void Dispose() { }
    }
}
