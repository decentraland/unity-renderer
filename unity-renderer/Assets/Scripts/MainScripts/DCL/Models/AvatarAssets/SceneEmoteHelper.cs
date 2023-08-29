using Cysharp.Threading.Tasks;
using DCL.Controllers;
using DCL.Emotes;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AvatarAssets
{
    public static class SceneEmoteHelper
    {
        public const string SCENE_EMOTE_PREFIX = "urn:decentraland:off-chain:scene-emote:";

        public static bool TryGenerateEmoteId(IParcelScene scene, string emoteFilePath, bool loop, out string emoteId)
        {
            if (scene.contentProvider.TryGetContentHash(emoteFilePath, out string emoteHash))
            {
                emoteId = $"{SCENE_EMOTE_PREFIX}{emoteHash}-{(loop ? "true" : "false")}";
                return true;
            }

            emoteId = string.Empty;
            return false;
        }

        public static bool TryGetDataFromEmoteId(string emoteId, out string emoteHash, out bool loop)
        {
            try
            {
                ReadOnlySpan<char> prefixRemoved = emoteId.AsSpan().Slice(SCENE_EMOTE_PREFIX.Length, emoteId.Length - SCENE_EMOTE_PREFIX.Length);
                int loopSeparator = prefixRemoved.LastIndexOf('-');
                emoteHash = prefixRemoved.Slice(0, loopSeparator).ToString();

                var loopSpan = prefixRemoved.Slice(loopSeparator + 1, 4);
                loop = loopSpan.Equals("true", StringComparison.InvariantCultureIgnoreCase);

                return true;
            }
            catch (Exception _)
            {
                emoteHash = string.Empty;
                loop = false;
            }

            return false;
        }

        public static bool IsSceneEmote(string emoteId) =>
            emoteId.StartsWith(SCENE_EMOTE_PREFIX);
    }
}
