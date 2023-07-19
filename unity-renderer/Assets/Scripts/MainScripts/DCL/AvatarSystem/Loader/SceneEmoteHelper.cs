using Cysharp.Threading.Tasks;
using DCL.Controllers;
using DCL.Emotes;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AvatarSystem
{
    public static class SceneEmoteHelper
    {
        public const string SCENE_EMOTE_PREFIX = "urn:decentraland:off-chain:scene-emote:";

        public static bool TryGenerateEmoteId(IParcelScene scene, string emoteFilePath, bool loop, out string emoteId)
        {
            if (scene.contentProvider.TryGetContentHash(emoteFilePath, out string emoteHash))
            {
                emoteId = $"{SCENE_EMOTE_PREFIX}:{emoteHash}-{(loop ? "true" : "false")}";
                return true;
            }

            emoteId = string.Empty;
            return false;
        }

        public static bool TryGetDataFromEmoteId(string emoteId, out string emoteHash, out bool loop)
        {
            try
            {
                ReadOnlySpan<char> prefixRemoved = emoteId.AsSpan().Slice(SCENE_EMOTE_PREFIX.Length + 1, emoteId.Length - SCENE_EMOTE_PREFIX.Length - 1);
                int loopSeparator = prefixRemoved.LastIndexOf('-');
                emoteHash = prefixRemoved.Slice(0, loopSeparator).ToString();

                loop = prefixRemoved.Slice(loopSeparator + 1, 3)
                                    .Equals("true", StringComparison.InvariantCultureIgnoreCase);

                return true;
            }
            catch (Exception _)
            {
                emoteHash = string.Empty;
                loop = false;
            }

            return false;
        }

        public static bool IsSceneEmote(string emoteId)
        {
            return emoteId.StartsWith(SCENE_EMOTE_PREFIX);
        }

        public static async UniTask RequestLoadSceneEmote(
            string bodyShapeId,
            string emoteId,
            IBaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData> animations,
            BaseRefCountedCollection<(string bodyshapeId, string emoteId)> emotesInUse,
            HashSet<(string bodyshapeId, string emoteId)> currentScenePendingSceneEmotes,
            HashSet<(string bodyshapeId, string emoteId)> currentSceneEquippedEmotes,
            Func<bool> cancelCondition,
            CancellationToken ct)
        {
            var emoteData = (bodyShapeId, emoteId);

            // check if already loaded
            if (currentSceneEquippedEmotes.Contains(emoteData))
            {
                return;
            }

            // if emote it's not pending to be resolved, we add it to the pending list
            if (!currentScenePendingSceneEmotes.Contains(emoteData))
            {
                currentScenePendingSceneEmotes.Add(emoteData);
                emotesInUse.IncreaseRefCount(emoteData);
            }

            // wait until emote is loaded
            while (!animations.ContainsKey(emoteData))
            {
                // if cancelled or scene was unloaded
                if (ct.IsCancellationRequested || cancelCondition())
                {
                    currentScenePendingSceneEmotes.Remove(emoteData);
                    emotesInUse.DecreaseRefCount(emoteData);
                    throw new OperationCanceledException(ct);
                }

                await UniTask.Yield(ct);
            }

            // flag it as equipped
            currentSceneEquippedEmotes.Add(emoteData);
        }
    }
}
