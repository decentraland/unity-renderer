using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Quests
{
    public class QuestDetailsComponentViewShould
    {
        private QuestDetailsComponentView questDetailsComponentView;

        [SetUp]
        public void SetUp()
        {
            questDetailsComponentView = Object.Instantiate(
                                                 AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/MainScripts/DCL/Controllers/HUD/Quest_V2/QuestDetails/QuestDetailsSection.prefab"))
                                            .GetComponent<QuestDetailsComponentView>();
        }

        [TearDown]
        public void TearDown()
        {
            questDetailsComponentView.Dispose();
            Object.Destroy(questDetailsComponentView);
        }

        [Test]
        public void SetQuestName()
        {
            questDetailsComponentView.SetQuestName("test name");

            Assert.AreEqual("test name", questDetailsComponentView.questName.text, "Quest name text does not match");
        }

        [Test]
        public void SetQuestCreator()
        {
            questDetailsComponentView.SetQuestCreator("creator test");

            Assert.AreEqual("creator test", questDetailsComponentView.questCreator.text, "Quest creator text does not match");
        }

        [Test]
        public void SetQuestDescription()
        {
            questDetailsComponentView.SetQuestDescription("this is a test description for the quest details panel");

            Assert.AreEqual("this is a test description for the quest details panel", questDetailsComponentView.questDescription.text,
                "Quest description text does not match");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetGuestSection(bool isGuest)
        {
            questDetailsComponentView.SetIsGuest(isGuest);

            Assert.AreEqual(isGuest, questDetailsComponentView.guestSection.activeSelf, "Guest section was not correctly toggled");
        }

        [Test]
        public void SetQuestRewards()
        {
            List<QuestRewardComponentModel> rewards = new List<QuestRewardComponentModel>()
            {
                new ()
                {
                    name = "reward1",
                    quantity = 30,
                    type = "helmet",
                    rarity = "uncommon",
                    imageUri = ""
                },
                new ()
                {
                    name = "reward2",
                    quantity = 3000,
                    type = "helmet",
                    rarity = "unique",
                    imageUri = ""
                },
            };
            questDetailsComponentView.SetQuestRewards(rewards);
            var activeRewards = 0;

            for (var i = 0; i < questDetailsComponentView.rewardsParent.childCount; i++)
                if (questDetailsComponentView.rewardsParent.GetChild(i).gameObject.activeSelf)
                    activeRewards++;

            Assert.AreEqual(2, activeRewards, "Quest rewards count do not match");
        }

        [Test]
        public void SetQuestSteps()
        {
            List<QuestStepComponentModel> steps = new List<QuestStepComponentModel>()
            {
                new ()
                {
                    text = "step A",
                    coordinates = new Vector2Int(0,3),
                    isCompleted = false,
                    supportsJumpIn = true
                },
                new ()
                {
                    text = "step B",
                    isCompleted = true,
                    supportsJumpIn = false
                },
            };
            questDetailsComponentView.SetQuestSteps(steps);
            var activeSteps = 0;

            for (var i = 0; i < questDetailsComponentView.stepsParent.childCount; i++)
                if (questDetailsComponentView.stepsParent.GetChild(i).gameObject.activeSelf)
                    activeSteps++;

            Assert.AreEqual(2, activeSteps, "Quest steps count do not match");
        }
    }
}
