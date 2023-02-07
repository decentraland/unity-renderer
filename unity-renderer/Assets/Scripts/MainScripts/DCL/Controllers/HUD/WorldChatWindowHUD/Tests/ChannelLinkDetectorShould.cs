using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Chat.HUD
{
    public class ChannelLinkDetectorShould
    {
        private ChannelLinkDetector channelLinkDetector;
        private TextMeshProUGUI textComponent;

        [SetUp]
        public void SetUp()
        {
            channelLinkDetector = new GameObject().AddComponent<ChannelLinkDetector>();
            textComponent = channelLinkDetector.gameObject.AddComponent<TextMeshProUGUI>();
            channelLinkDetector.textComponent = textComponent;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(channelLinkDetector.gameObject);
        }

        [UnityTest]
        public IEnumerator DetectChannelsPattersInNormalTextCorrectly() =>
            UniTask.ToCoroutine(async () =>
            {
                // Arrange
                textComponent.text = "This is a #test with several channels: #channel1, #channel2, ~nearby. End of the string.";
                channelLinkDetector.hasNoParseLabel = false;

                // Act
                await channelLinkDetector.RefreshChannelPatterns(default);

                // Assert
                string newTextToAssert = "This is a <link=#test><color=#4886E3><u>#test</u></color></link> with several channels: " +
                                         "<link=#channel1><color=#4886E3><u>#channel1</u></color></link>, " +
                                         "<link=#channel2><color=#4886E3><u>#channel2</u></color></link>, " +
                                         "<link=~nearby><color=#4886E3><u>~nearby</u></color></link>. " +
                                         "End of the string.";

                Assert.AreEqual(newTextToAssert, textComponent.text);
                Assert.AreEqual(newTextToAssert, channelLinkDetector.currentText);
                Assert.AreEqual(4, channelLinkDetector.channelsFoundInText.Count);
            });

        [UnityTest]
        public IEnumerator DetectChannelsPattersInNoParseTextCorrectly() =>
            UniTask.ToCoroutine(async () =>
            {
                // Arrange
                textComponent.text = "<noparse>This is a #test with several channels: #channel1, #channel2, ~nearby. End of the string.</noparse>";
                channelLinkDetector.hasNoParseLabel = true;

                // Act
                await channelLinkDetector.RefreshChannelPatterns(default);

                // Assert
                string newTextToAssert = "<noparse>This is a </noparse><link=#test><color=#4886E3><u>#test</u></color></link><noparse> with several channels: " +
                                         "</noparse><link=#channel1><color=#4886E3><u>#channel1</u></color></link><noparse>, " +
                                         "</noparse><link=#channel2><color=#4886E3><u>#channel2</u></color></link><noparse>, " +
                                         "</noparse><link=~nearby><color=#4886E3><u>~nearby</u></color></link><noparse>. " +
                                         "End of the string.</noparse>";

                Assert.AreEqual(newTextToAssert, textComponent.text);
                Assert.AreEqual(newTextToAssert, channelLinkDetector.currentText);
                Assert.AreEqual(4, channelLinkDetector.channelsFoundInText.Count);
            });
    }
}
