using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DCL.Quests
{
    public class QuestCompletedComponentViewShould
    {
        private QuestCompletedComponentView questCompletedComponentView;

        [SetUp]
        public void SetUp()
        {
            questCompletedComponentView = Object.Instantiate(
                                                 AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/MainScripts/DCL/Controllers/HUD/Quest_V2/QuestCompletedHUD/QuestCompletedHUD.prefab"))
                                            .GetComponent<QuestCompletedComponentView>();
        }

        [TearDown]
        public void TearDown()
        {
            questCompletedComponentView.Dispose();
            Object.Destroy(questCompletedComponentView.gameObject);
        }

        [Test]
        public void SetQuestTitle()
        {
            questCompletedComponentView.SetTitle("test title");

            Assert.AreEqual("test title", questCompletedComponentView.questTitle.text, "Quest title text does not match");
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
            questCompletedComponentView.SetRewards(rewards);
            var activeRewards = 0;

            for (var i = 0; i < questCompletedComponentView.rewardsContainer.childCount; i++)
                if (questCompletedComponentView.rewardsContainer.GetChild(i).gameObject.activeSelf)
                    activeRewards++;

            Assert.AreEqual(2, activeRewards, "Quest rewards count do not match");
        }

        [Test]
        public void HideRewardsSectionIfEmpty()
        {
            questCompletedComponentView.SetRewards(new List<QuestRewardComponentModel>());

            Assert.False(questCompletedComponentView.rewardsSection.activeInHierarchy, "Reward section is active even if empty");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetGuestSection(bool isGuest)
        {
            questCompletedComponentView.SetIsGuest(isGuest);

            Assert.AreEqual(isGuest, questCompletedComponentView.guestSection.activeInHierarchy, "Guest section was not correctly set");
        }
    }
}
