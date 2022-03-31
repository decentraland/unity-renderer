using System;
using System.Collections.Generic;

namespace AvatarSystem
{
    public interface IEmoteAnimationEquipper : IDisposable
    {
        void SetEquippedEmotes( string bodyShapeId, IEnumerable<WearableItem> emotes);
    }
}