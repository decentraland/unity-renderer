using DCL.Emotes;
using NUnit.Framework;
using UnityEditor;

namespace AvatarAssets.Tests
{
    [Category("EditModeCI")]
    public class EmbeddedEmotesShould
    {
        [Test]
        public void ValidateEmbeddedEmotesData()
        {
            var assetPath = "Assets/DCLServices/EmotesService/Addressables/EmbeddedEmotes.asset";
            var asset = AssetDatabase.LoadAssetAtPath<EmbeddedEmotesSO>(assetPath);

            Assert.NotNull(asset);

            EmbeddedEmote[] embeddedEmotes = asset.GetEmbeddedEmotes();

            // Check the count of embedded emotes
            Assert.AreEqual(embeddedEmotes.Length, 33);

            // Ensure that no emote ID contains an underscore
            // validation on catalyst to save the profile will reject the request
            foreach (var emote in embeddedEmotes)
                Assert.IsFalse(emote.id.Contains("_"), $"Emote ID '{emote.id}' should not contain an underscore.");
        }
    }
}
