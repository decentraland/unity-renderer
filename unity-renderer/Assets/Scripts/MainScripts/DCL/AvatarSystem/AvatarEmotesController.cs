﻿using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using DCL.Emotes;
using DCL.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace AvatarSystem
{
    public class AvatarEmotesController : IAvatarEmotesController
    {
        public event Action<string, IEmoteReference> OnEmoteEquipped;
        public event Action<string> OnEmoteUnequipped;


        private string bodyShapeId = "";
        private readonly IAnimator animator;
        private readonly IEmotesService emotesService;
        private readonly Dictionary<EmoteBodyId, IEmoteReference> equippedEmotes = new ();
        private readonly CancellationTokenSource cts = new ();

        public AvatarEmotesController(IAnimator animator, IEmotesService emotesService)
        {
            this.animator = animator;
            this.emotesService = emotesService;
        }

        public bool TryGetEquippedEmote(string bodyShape, string emoteId, out IEmoteReference emoteReference) =>
            equippedEmotes.TryGetValue(new EmoteBodyId(bodyShape, emoteId), out emoteReference);

        // ReSharper disable once PossibleMultipleEnumeration (its intended)
        public void LoadEmotes(string bodyShapeId, IEnumerable<WearableItem> newEmotes)
        {
            this.bodyShapeId = bodyShapeId;

            foreach (WearableItem emote in newEmotes)
                LoadEmote(bodyShapeId, emote);
        }

        private void LoadEmote(string bodyShapeId, WearableItem emote)
        {
            var emoteKey = new EmoteBodyId(bodyShapeId, emote.id);
            if (equippedEmotes.ContainsKey(emoteKey)) return;
            equippedEmotes.Add(emoteKey, null);
            AsyncEmoteLoad(bodyShapeId, emote.id).Forget();
        }

        private async UniTask AsyncEmoteLoad(string bodyShapeId, string emoteId)
        {
            var emoteKey = new EmoteBodyId(bodyShapeId, emoteId);

            try
            {
                IEmoteReference emoteReference = await emotesService.RequestEmote(emoteKey, cts.Token);
                animator.EquipEmote(emoteId, emoteReference.GetData());
                equippedEmotes[emoteKey] = emoteReference;
                OnEmoteEquipped?.Invoke(emoteId, emoteReference);
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void PlayEmote(string emoteId, long timestamps, bool spatialSound)
        {
            if (string.IsNullOrEmpty(emoteId)) return;

            var emoteKey = new EmoteBodyId(bodyShapeId, emoteId);

            if (equippedEmotes.ContainsKey(emoteKey))
                animator.PlayEmote(emoteId, timestamps, spatialSound);
        }

        public void StopEmote()
        {
            animator.StopEmote();
        }

        public void EquipEmote(string emoteId, IEmoteReference emoteReference)
        {
            var emoteKey = new EmoteBodyId(bodyShapeId, emoteId);

            if (equippedEmotes.ContainsKey(emoteKey))
            {
                // we avoid dangling references in case an emote was loaded twice
                emoteReference.Dispose();
                return;
            }

            equippedEmotes.Add(emoteKey, emoteReference);
            animator.EquipEmote(emoteId, emoteReference.GetData());
            OnEmoteEquipped?.Invoke(emoteId, emoteReference);
        }

        public void UnEquipEmote(string emoteId)
        {
            var emoteKey = new EmoteBodyId(bodyShapeId, emoteId);

            if (equippedEmotes.ContainsKey(emoteKey))
            {
                animator.UnequipEmote(emoteId);
                equippedEmotes[emoteKey].Dispose();
                equippedEmotes.Remove(emoteKey);
            }
        }


        public void Dispose()
        {
            cts.SafeCancelAndDispose();

            foreach (var kvp in equippedEmotes)
                kvp.Value.Dispose();

            equippedEmotes.Clear();
        }
    }
}
