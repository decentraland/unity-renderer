using NUnit.Framework;
using UnityEngine;

namespace DCL.Quest
{
    public class QuestStepComponentViewTest
    {
        private QuestStepComponentView questStepComponentView;

        [SetUp]
        public void SetUp()
        {
            questStepComponentView = BaseComponentView.Create<QuestStepComponentView>("QuestStep");
        }

        [TearDown]
        public void TearDown()
        {
            questStepComponentView.Dispose();
            Object.Destroy(questStepComponentView);
        }

        [Test]
        public void SetText()
        {
            var questStepText = "template text";
            questStepComponentView.SetQuestStepText(questStepText);

            Assert.AreEqual(questStepText, questStepComponentView.questStepText.text);
        }

        [Test]
        public void SetCompleted()
        {
            var questStepText = "template text";
            questStepComponentView.SetQuestStepText(questStepText);
            questStepComponentView.SetIsCompleted(true);

            Assert.True(questStepComponentView.completedQuestToggle.activeSelf);
            Assert.False(questStepComponentView.nonCompletedQuestToggle.activeSelf);
            Assert.AreEqual(questStepComponentView.questStepText.color, questStepComponentView.completedTextColor);
            Assert.AreEqual(questStepText, questStepComponentView.questStepText.text);
        }

        [Test]
        public void SetNonCompleted()
        {
            var questStepText = "template text";
            questStepComponentView.SetQuestStepText(questStepText);
            questStepComponentView.SetIsCompleted(false);

            Assert.False(questStepComponentView.completedQuestToggle.activeSelf);
            Assert.True(questStepComponentView.nonCompletedQuestToggle.activeSelf);
            Assert.AreEqual(questStepComponentView.questStepText.color, questStepComponentView.normalTextColor);
            Assert.AreEqual(questStepText, questStepComponentView.questStepText.text);
        }
    }
}
