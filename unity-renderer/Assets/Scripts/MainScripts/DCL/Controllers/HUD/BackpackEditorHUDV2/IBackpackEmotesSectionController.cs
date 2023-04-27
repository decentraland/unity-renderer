using System;

namespace DCL.Backpack
{
    public interface IBackpackEmotesSectionController
    {
        event Action<string> OnNewEmoteAdded;
        event Action<string> OnEmotePreviewed;

        void LoadEmotes();
        void RestoreEmoteSlots();
        void SetEquippedBodyShape(string bodyShapeId);
        void Dispose();
    }
}
