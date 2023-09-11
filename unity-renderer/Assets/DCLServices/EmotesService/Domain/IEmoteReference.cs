using System;

namespace DCL.Emotes
{
    public interface IEmoteReference : IDisposable
    {
        WearableItem GetEntity();
        EmoteClipData GetData();
    }
}
