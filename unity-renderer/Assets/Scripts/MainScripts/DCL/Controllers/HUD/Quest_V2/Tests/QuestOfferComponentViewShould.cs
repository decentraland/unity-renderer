using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Quest
{
    public class QuestOfferComponentViewShould
    {
        private QuestOfferComponentView questOfferComponentView;

        [SetUp]
        public void SetUp()
        {
            questOfferComponentView = Object.Instantiate(
                                                 AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/MainScripts/DCL/Controllers/HUD/Quest_V2/QuestOfferHUD/QuestOfferHUD.prefab"))
                                            .GetComponent<QuestOfferComponentView>();
        }

        [TearDown]
        public void TearDown()
        {
            questOfferComponentView.Dispose();
            Object.Destroy(questOfferComponentView);
        }

        [Test]
        public void SetQuestTitle()
        {
            questOfferComponentView.SetQuestTitle("test title");

            Assert.AreEqual("test title", questOfferComponentView.questTitle.text, "Quest title text does not match");
        }

        [Test]
        public void SetQuestDescription()
        {
            questOfferComponentView.SetQuestDescription("test description for this quest");

            Assert.AreEqual("test description for this quest", questOfferComponentView.questDescription.text, "Quest description text does not match");
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
            questOfferComponentView.SetRewards(rewards);


            Assert.AreEqual(2, questOfferComponentView.rewardsContainer.childCount, "Quest rewards count do not match");
        }

        [Test]
        public void HideRewardsSectionIfEmpty()
        {
            questOfferComponentView.SetRewards(new List<QuestRewardComponentModel>());

            Assert.False(questOfferComponentView.rewardsSection.activeInHierarchy, "Reward section is active even if empty");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetGuestSection(bool isGuest)
        {
            questOfferComponentView.SetIsGuest(isGuest);

            Assert.AreEqual(isGuest, questOfferComponentView.guestSection.activeInHierarchy, "Guest section was not correctly set");
        }
    }
}
