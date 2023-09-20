using DCLServices.EmotesService.Domain;
using System;

namespace DCL.Emotes
{
    public class EmbedEmoteReference : IEmoteReference
    {
        private readonly WearableItem emoteItem;
        private readonly EmoteAnimationData animationData;

        public EmbedEmoteReference(WearableItem emoteItem, EmoteAnimationData animationData)
        {
            this.emoteItem = emoteItem;
            this.animationData = animationData;
        }

        public WearableItem GetEntity() =>
            emoteItem;

        public EmoteAnimationData GetData() =>
            animationData;

        public void Dispose()
        {

        }
    }
}
