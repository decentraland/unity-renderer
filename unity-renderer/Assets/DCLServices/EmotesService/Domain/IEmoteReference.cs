using DCLServices.EmotesService.Domain;
using System;

namespace DCL.Emotes
{
    public interface IEmoteReference : IDisposable
    {
        WearableItem GetEntity();
        EmoteAnimationData GetData();
    }
}
