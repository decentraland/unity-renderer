using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Quests
{
    public class QuestTrackerComponentViewShould
    {
        private QuestTrackerComponentView questTrackerComponentView;

        [SetUp]
        public void SetUp()
        {
            questTrackerComponentView = BaseComponentView.Create<QuestTrackerComponentView>("QuestTrackerHUD");
        }

        [TearDown]
        public void TearDown()
        {
            questTrackerComponentView.Dispose();
            Object.Destroy(questTrackerComponentView);
        }

        [Test]
        public void UpdateQuestTitle()
        {
            questTrackerComponentView.SetQuestTitle("test title");

            Assert.AreEqual("test title", questTrackerComponentView.questTitleText.text, "Quest title text does not match");
        }

        [Test]
        public void SetQuestSteps()
        {
            List<QuestStepComponentModel> questSteps = new List<QuestStepComponentModel>()
            {
                new ()
                {
                    text = "Step 1",
                    isCompleted = false
                },
                new ()
                {
                    text = "Step 2",
                    isCompleted = true
                }
            };
            questTrackerComponentView.SetQuestSteps(questSteps);

            Assert.AreEqual(2, questTrackerComponentView.currentQuestSteps.Count, "Expecting only 2 quest steps");
            Assert.AreEqual(3, questTrackerComponentView.stepsContainer.childCount, "Expecting only 3 childs: quest title and 2 steps");
        }

        [Test]
        public void ClickJumpIn()
        {
            Vector2Int clickedCoordinates = new Vector2Int(0,0);
            Vector2Int finalCoordinates = new Vector2Int(10, 33);
            questTrackerComponentView.SetQuestCoordinates(finalCoordinates);
            questTrackerComponentView.OnJumpIn += (x) => clickedCoordinates = x;
            questTrackerComponentView.jumpInButton.onClick?.Invoke();

            Assert.AreEqual(finalCoordinates, clickedCoordinates);
        }


        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetJumpInButtonSupport(bool supports)
        {
            questTrackerComponentView.SetSupportsJumpIn(supports);

            Assert.AreEqual(supports, questTrackerComponentView.jumpInButton.gameObject.activeSelf);
        }
    }
}
