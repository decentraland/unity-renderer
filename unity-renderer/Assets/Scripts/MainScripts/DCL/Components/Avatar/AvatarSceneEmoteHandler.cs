using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL.Emotes;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL
{
    public class AvatarSceneEmoteHandler
    {
        private readonly IAvatar avatar;
        private readonly IBaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData> alreadyLoadedEmotes;
        private readonly BaseRefCountedCollection<(string bodyshapeId, string emoteId)> emotesInUse;
        private readonly HashSet<(string bodyshapeId, string emoteId)> pendingEmotesByScene;
        private readonly HashSet<(string bodyshapeId, string emoteId)> equippedEmotesByScene;

        private long lamportTimestamp = 0;
        internal CancellationTokenSource cancellationTokenSource = null;

        public AvatarSceneEmoteHandler(IAvatar avatar,
            IBaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData> alreadyLoadedEmotes,
            BaseRefCountedCollection<(string bodyshapeId, string emoteId)> emotesInUse)
        {
            this.avatar = avatar;
            this.alreadyLoadedEmotes = alreadyLoadedEmotes;
            this.emotesInUse = emotesInUse;
            this.equippedEmotesByScene = new HashSet<(string bodyshapeId, string emoteId)>();
            this.pendingEmotesByScene = new HashSet<(string bodyshapeId, string emoteId)>();
        }

        public bool IsSceneEmote(string emoteId)
        {
            return SceneEmoteHelper.IsSceneEmote(emoteId);
        }

        public void SetExpressionLamportTimestamp(long timestamp)
        {
            lamportTimestamp = timestamp;
        }

        public async UniTask LoadAndPlayEmote(string bodyShapeId, string emoteId)
        {
            long timestamp = lamportTimestamp;
            cancellationTokenSource ??= new CancellationTokenSource();

            try
            {
                await SceneEmoteHelper.RequestLoadSceneEmote(
                    bodyShapeId,
                    emoteId,
                    alreadyLoadedEmotes,
                    emotesInUse,
                    pendingEmotesByScene,
                    equippedEmotesByScene,
                    cancellationTokenSource.Token
                );

                //avoid playing emote if timestamp has change,
                //meaning a new emote was trigger while this one was loading
                if (timestamp == lamportTimestamp)
                {
                    avatar.EquipEmote(emoteId, alreadyLoadedEmotes[(bodyShapeId, emoteId)]);
                    avatar.PlayEmote(emoteId, lamportTimestamp);
                }
            }
            catch (OperationCanceledException _)
            {
                // Ignored
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void CleanUp()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }

            foreach (var emoteData in equippedEmotesByScene)
            {
                avatar?.UnequipEmote(emoteData.emoteId);
                emotesInUse.DecreaseRefCount(emoteData);
            }

            foreach (var emoteData in pendingEmotesByScene)
            {
                emotesInUse.DecreaseRefCount(emoteData);
            }

            equippedEmotesByScene.Clear();
            pendingEmotesByScene.Clear();
        }
    }
}
