using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Emotes;
using UnityEngine;

namespace AvatarSystem
{
    public class EmoteAnimationEquipper : IEmoteAnimationEquipper
    {
        internal readonly IAnimator animator;
        internal readonly DataStore_Emotes dataStoreEmotes;

        internal string bodyShapeId = "";
        internal readonly List<string> emotes = new List<string>();

        public EmoteAnimationEquipper(IAnimator animator, DataStore_Emotes dataStoreEmotes)
        {
            this.animator = animator;
            this.dataStoreEmotes = dataStoreEmotes;
            this.dataStoreEmotes.animations.OnAdded += OnAnimationAdded;
            this.dataStoreEmotes.animations.OnRemoved += OnAnimationRemoved;
        }

        private void OnAnimationAdded((string bodyshapeId, string emoteId) values, EmoteClipData emoteClipData)
        {
            if (bodyShapeId != values.bodyshapeId)
                return;

            if (!emotes.Contains(values.emoteId))
                return;
            animator.EquipEmote(values.emoteId, emoteClipData);
        }

        private void OnAnimationRemoved((string bodyshapeId, string emoteId) values, EmoteClipData emoteClipData)
        {
            if (bodyShapeId != values.bodyshapeId)
                return;

            animator.UnequipEmote(values.emoteId);
        }

        public void SetEquippedEmotes(string bodyShapeId, IEnumerable<WearableItem> newEmotes)
        {
            this.bodyShapeId = bodyShapeId;
            foreach (WearableItem emote in newEmotes)
            {
                dataStoreEmotes.emotesOnUse.IncreaseRefCount((bodyShapeId, emote.id));

                //If the clip is not ready by the time we equip it
                //we will receive it once its added to the collection in the DataStore
                if (dataStoreEmotes.animations.TryGetValue((this.bodyShapeId, emote.id),
                        out EmoteClipData emoteClipData))
                    animator.EquipEmote(emote.id, emoteClipData);
            }

            foreach (string emoteId in this.emotes)
            {
                dataStoreEmotes.emotesOnUse.DecreaseRefCount((this.bodyShapeId, emoteId));
            }
            this.emotes.Clear();
            this.emotes.AddRange(newEmotes.Select(e => e.id));
        }

        public void Dispose()
        {
            dataStoreEmotes.animations.OnAdded -= OnAnimationAdded;
            dataStoreEmotes.animations.OnRemoved -= OnAnimationRemoved;
            foreach (string emoteId in this.emotes)
            {
                dataStoreEmotes.emotesOnUse.DecreaseRefCount((bodyShapeId, emoteId));
            }
            emotes.Clear();
        }
    }
}