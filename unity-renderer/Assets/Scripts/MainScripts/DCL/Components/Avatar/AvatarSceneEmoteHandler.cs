using AvatarAssets;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL.Emotes;
using DCL.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL
{
    public class AvatarSceneEmoteHandler
    {
        private readonly IAvatarEmotesController emotesController;
        private readonly IEmotesService emotesService;
        private readonly HashSet<string> equippedEmotes;

        private long lamportTimestamp;
        private CancellationTokenSource cts;

        public AvatarSceneEmoteHandler(IAvatarEmotesController emotesController, IEmotesService emotesService)
        {
            this.emotesController = emotesController;
            this.emotesService = emotesService;
            equippedEmotes = new HashSet<string>();
        }

        public bool IsSceneEmote(string emoteId) =>
            SceneEmoteHelper.IsSceneEmote(emoteId);

        public void SetExpressionLamportTimestamp(long timestamp)
        {
            lamportTimestamp = timestamp;
        }

        public async UniTask LoadAndPlayEmote(string bodyShapeId, string emoteId)
        {
            long timestamp = lamportTimestamp;
            cts ??= new CancellationTokenSource();

            var emoteKey = new EmoteBodyId(bodyShapeId, emoteId);

            if (equippedEmotes.Contains(emoteId))
            {
                TriggerEmote(emoteId, timestamp);
                return;
            }

            try
            {
                var loadedEmote = await emotesService.RequestEmote(emoteKey, cts.Token);

                emotesController.EquipEmote(emoteId, loadedEmote);
                equippedEmotes.Add(emoteId);

                TriggerEmote(emoteId, timestamp);
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { Debug.LogException(e); }
        }

        private void TriggerEmote(string emoteId, long timestamp)
        {
            //avoid playing emote if timestamp has change,
            //meaning a new emote was trigger while this one was loading
            if (timestamp == lamportTimestamp)
                emotesController.PlayEmote(emoteId, lamportTimestamp);
        }

        public void CleanUp()
        {
            cts?.SafeCancelAndDispose();
            cts = null;

            foreach (string emoteId in equippedEmotes)
                emotesController?.UnEquipEmote(emoteId);

            equippedEmotes.Clear();
        }
    }
}
