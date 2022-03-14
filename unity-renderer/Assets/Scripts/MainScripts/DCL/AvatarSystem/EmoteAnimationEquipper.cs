using System.Collections.Generic;
using DCL;
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

        private void OnAnimationAdded((string bodyshapeId, string emoteId) values, AnimationClip animationClip)
        {
            if (bodyShapeId != values.bodyshapeId)
                return;

            if (!emotes.Contains(values.emoteId))
                return;

            animator.EquipEmote(values.emoteId, animationClip);
        }

        private void OnAnimationRemoved((string bodyshapeId, string emoteId) values, AnimationClip animationClip)
        {
            if (bodyShapeId != values.bodyshapeId)
                return;

            animator.UnequipEmote(values.emoteId);
        }

        public void SetEquippedEmotes(string bodyShapeId, IEnumerable<WearableItem> emotes)
        {
            this.bodyShapeId = bodyShapeId;
            foreach (string emoteId in this.emotes)
            {
                dataStoreEmotes.emotesOnUse.DecreaseRefCount(emoteId);
            }
            this.emotes.Clear();

            foreach (WearableItem emote in emotes)
            {
                this.emotes.Add(emote.id);
                dataStoreEmotes.emotesOnUse.IncreaseRefCount(emote.id);

                //If the clip is not ready by the time we equip it
                //we will receive it once its added to the collection in the DataStore
                if (dataStoreEmotes.animations.TryGetValue((this.bodyShapeId, emote.id), out AnimationClip clip))
                    animator.EquipEmote(emote.id, clip);
            }
        }

        public void Dispose()
        {
            dataStoreEmotes.animations.OnAdded -= OnAnimationAdded;
            dataStoreEmotes.animations.OnRemoved -= OnAnimationRemoved;
            foreach (string emoteId in this.emotes)
            {
                dataStoreEmotes.emotesOnUse.DecreaseRefCount(emoteId);
            }
            emotes.Clear();
        }
    }
}