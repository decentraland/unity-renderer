using Cysharp.Threading.Tasks;
using DCL.Emotes;
using DCL.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace AvatarSystem
{
    public class AvatarEmotesController : IAvatarEmotesController
    {
        private const string IN_HIDE_AREA = "IN_HIDE_AREA";

        public event Action<string, IEmoteReference> OnEmoteEquipped;
        public event Action<string> OnEmoteUnequipped;

        private string bodyShapeId = "";
        private readonly IAnimator animator;
        private readonly IEmotesService emotesService;
        private readonly Dictionary<EmoteBodyId, IEmoteReference> equippedEmotes = new ();
        private readonly CancellationTokenSource cts = new ();
        private readonly HashSet<string> visibilityConstraints;

        public AvatarEmotesController(IAnimator animator, IEmotesService emotesService)
        {
            this.animator = animator;
            this.emotesService = emotesService;
            visibilityConstraints = new HashSet<string>();
        }

        public bool TryGetEquippedEmote(string bodyShape, string emoteId, out IEmoteReference emoteReference) =>
            equippedEmotes.TryGetValue(new EmoteBodyId(bodyShape, emoteId), out emoteReference);

        public void AddVisibilityConstraint(string key)
        {
            visibilityConstraints.Add(key);

            if (!CanPlayEmote())
                StopEmote(true);
        }

        public void RemoveVisibilityConstraint(string key)
        {
            visibilityConstraints.Remove(key);
        }

        public void Prepare(string bodyShapeId, GameObject container)
        {
            this.bodyShapeId = bodyShapeId;
            animator.Prepare(bodyShapeId, container);
        }

        // ReSharper disable once PossibleMultipleEnumeration (its intended)
        public void LoadEmotes(string bodyShapeId, IEnumerable<WearableItem> newEmotes)
        {
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
                if (emoteReference == null) return;
                animator.EquipEmote(emoteId, emoteReference.GetData());
                equippedEmotes[emoteKey] = emoteReference;
                OnEmoteEquipped?.Invoke(emoteId, emoteReference);
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { Debug.LogException(e); }
        }

        public void PlayEmote(string emoteId, long timestamps, bool spatial = true, bool occlude = true, bool forcePlay = false)
        {
            bool isPlayingEmote = !string.IsNullOrEmpty(animator.GetCurrentEmoteId());
            bool emoteIsValid = !string.IsNullOrEmpty(emoteId);

            if (isPlayingEmote && !emoteIsValid)
                animator.StopEmote(false);

            if (!emoteIsValid) return;
            if (!CanPlayEmote()) return;

            var emoteKey = new EmoteBodyId(bodyShapeId, emoteId);
            if (!equippedEmotes.ContainsKey(emoteKey)) return;

            animator.PlayEmote(emoteId, timestamps, spatial, occlude, forcePlay);
        }

        private bool CanPlayEmote() =>
            !visibilityConstraints.Contains(IN_HIDE_AREA);

        public void StopEmote(bool immediate)
        {
            animator.StopEmote(immediate);
        }

        public void EquipEmote(string emoteId, IEmoteReference emoteReference)
        {
            if (emoteReference == null) return;
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
