using Cysharp.Threading.Tasks;
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
        private readonly Dictionary<(string, string), IEmoteReference> emotes = new ();
        private readonly CancellationTokenSource cts = new ();

        public AvatarEmotesController(IAnimator animator, IEmotesService emotesService)
        {
            this.animator = animator;
            this.emotesService = emotesService;
        }

        public bool TryGetEquippedEmote(string bodyShape, string emoteId, out IEmoteReference emoteReference) =>
            emotes.TryGetValue((bodyShape, emoteId), out emoteReference);

        // ReSharper disable once PossibleMultipleEnumeration (its intended)
        public void LoadEmotes(string bodyShapeId, IEnumerable<WearableItem> newEmotes)
        {
            this.bodyShapeId = bodyShapeId;

            foreach (WearableItem emote in newEmotes)
                LoadEmote(bodyShapeId, emote);
        }

        private void LoadEmote(string bodyShapeId, WearableItem emote)
        {
            (string bodyShapeId, string id) emoteKey = (bodyShapeId, emote.id);
            if (emotes.ContainsKey(emoteKey)) return;
            emotes.Add(emoteKey, null);
            AsyncEmoteLoad(bodyShapeId, emote.id).Forget();
        }

        private async UniTask AsyncEmoteLoad(string bodyShapeId, string emoteId)
        {
            (string bodyShapeId, string emoteId) emoteKey = (bodyShapeId, emoteId);

            try
            {
                IEmoteReference emoteReference = await emotesService.RequestEmote(bodyShapeId, emoteId, cts.Token);
                animator.EquipEmote(emoteId, emoteReference.GetData());
                emotes[emoteKey] = emoteReference;
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

            (string bodyShapeId, string emoteId) emoteKey = (bodyShapeId, emoteId);

            if (emotes.ContainsKey(emoteKey))
                animator.PlayEmote(emoteId, timestamps, spatialSound);
        }

        public void EquipEmote(string emoteId, IEmoteReference emoteReference)
        {
            if (emotes.ContainsKey((bodyShapeId, emoteId)))
            {
                // we avoid dangling references in case an emote was loaded twice
                emoteReference.Dispose();
                return;
            }

            emotes.Add((bodyShapeId, emoteId), emoteReference);
            animator.EquipEmote(emoteId, emoteReference.GetData());
            OnEmoteEquipped?.Invoke(emoteId, emoteReference);
        }

        public void UnEquipEmote(string emoteId)
        {
            (string bodyShapeId, string emoteId) emoteKey = (bodyShapeId, emoteId);

            if (emotes.ContainsKey(emoteKey))
            {
                animator.UnequipEmote(emoteId);
                emotes[emoteKey].Dispose();
                emotes.Remove(emoteKey);
            }
        }


        public void Dispose()
        {
            cts.SafeCancelAndDispose();

            foreach (var kvp in emotes)
                kvp.Value.Dispose();

            emotes.Clear();
        }
    }
}
