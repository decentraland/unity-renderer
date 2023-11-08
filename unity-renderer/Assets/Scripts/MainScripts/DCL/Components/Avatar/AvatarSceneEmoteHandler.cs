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
        private readonly HashSet<EmoteBodyId> equippedEmotes;

        private long lamportTimestamp;
        internal CancellationTokenSource cts;

        public AvatarSceneEmoteHandler(IAvatarEmotesController emotesController, IEmotesService emotesService)
        {
            this.emotesController = emotesController;
            this.emotesService = emotesService;
            this.equippedEmotes = new HashSet<EmoteBodyId>();
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

            try
            {
                var loadedEmote = await emotesService.RequestEmote(emoteKey, cts.Token);

                emotesController.EquipEmote(emoteId, loadedEmote);
                equippedEmotes.Add(emoteKey);

                //avoid playing emote if timestamp has change,
                //meaning a new emote was trigger while this one was loading
                if (timestamp == lamportTimestamp)
                {
                    emotesController.PlayEmote(emoteId, lamportTimestamp);
                    UserProfile.GetOwnUserProfile().SetAvatarExpression(emoteId, UserProfile.EmoteSource.EmoteLoop, true);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { Debug.LogException(e); }
        }

        public void CleanUp()
        {
            cts?.SafeCancelAndDispose();
            cts = null;

            foreach (var emoteData in equippedEmotes)
                emotesController?.UnEquipEmote(emoteData.EmoteId);

            equippedEmotes.Clear();
        }
    }
}
