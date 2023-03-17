// TODO: make this tests work
/*
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DCL.Social.Chat.Mentions.Tests
{
    public class MentionLinkDetectorShould
    {
        private MentionLinkDetector mentionLinkDetector;
        private TextMeshProUGUI testTextComponent;

        [SetUp]
        public void SetUp()
        {
            mentionLinkDetector = new GameObject().AddComponent<MentionLinkDetector>();
            testTextComponent = mentionLinkDetector.gameObject.AddComponent<TextMeshProUGUI>();
            mentionLinkDetector.textComponent = testTextComponent;
            mentionLinkDetector.isMentionsFeatureEnabled = true;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(mentionLinkDetector.gameObject);
        }

        [UnityTest]
        public IEnumerator DetectMentionsPattersInNormalTextCorrectly() =>
            UniTask.ToCoroutine(async () =>
            {
                // Arrange
                testTextComponent.text = "This is a test with several mentions: @mention1, @mention2, @mention3. End of the string.";
                mentionLinkDetector.hasNoParseLabel = false;

                // Act
                await mentionLinkDetector.RefreshMentionPatterns(default(CancellationToken));

                // Assert
                string newTextToAssert = "This is a test with several mentions: " +
                                         "<link=mention://mention1><color=#4886E3><u>@mention1</u></color></link>, " +
                                         "<link=mention://mention2><color=#4886E3><u>@mention2</u></color></link>, " +
                                         "<link=mention://mention3><color=#4886E3><u>@mention3</u></color></link>. " +
                                         "End of the string.";

                Assert.AreEqual(newTextToAssert, testTextComponent.text);
                Assert.AreEqual(newTextToAssert, mentionLinkDetector.currentText);
            });

        [UnityTest]
        public IEnumerator DetectMentionsPattersInNoParseTextCorrectly() =>
            UniTask.ToCoroutine(async () =>
            {
                // Arrange
                mentionLinkDetector.hasNoParseLabel = true;
                testTextComponent.text = "<noparse>This is a test with several mentions: @mention1, @mention2, @mention3. End of the string.</noparse>";

                // Act
                await mentionLinkDetector.RefreshMentionPatterns(default(CancellationToken));

                // Assert
                string newTextToAssert = "<noparse>This is a test with several mentions: " +
                                         "</noparse><link=mention://mention1><color=#4886E3><u>@mention1</u></color></link><noparse>, " +
                                         "</noparse><link=mention://mention2><color=#4886E3><u>@mention2</u></color></link><noparse>, " +
                                         "</noparse><link=mention://mention3><color=#4886E3><u>@mention3</u></color></link><noparse>. " +
                                         "End of the string.</noparse>";

                Assert.AreEqual(newTextToAssert, testTextComponent.text);
                Assert.AreEqual(newTextToAssert, mentionLinkDetector.currentText);
            });
    }
}
*/
