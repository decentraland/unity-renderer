using System;
using System.Collections.Generic;

namespace AvatarSystem
{
    public interface IEmoteAnimationEquipper : IDisposable
    {
        void SetEquippedEmotes( string bodyshapeId, IEnumerable<WearableItem> emotes);
    }
}