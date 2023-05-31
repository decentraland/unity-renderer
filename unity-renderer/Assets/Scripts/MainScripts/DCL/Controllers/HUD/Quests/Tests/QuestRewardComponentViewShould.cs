using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Quests
{
    public class QuestRewardComponentViewShould
    {
        private QuestRewardComponentView questRewardComponentView;

        [SetUp]
        public void SetUp()
        {
            questRewardComponentView = Object.Instantiate(
                                                 AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/MainScripts/DCL/Controllers/HUD/Quest_V2/QuestReward/QuestReward.prefab"))
                                            .GetComponent<QuestRewardComponentView>();
        }

        [TearDown]
        public void TearDown()
        {
            questRewardComponentView.Dispose();
            Object.Destroy(questRewardComponentView);
        }

        [Test]
        public void SetRewardName()
        {
            questRewardComponentView.SetName("test name");

            Assert.AreEqual("test name", questRewardComponentView.rewardName.text, "Reward name text does not match");
        }

        [Test]
        public void SetRewardQuantity()
        {
            questRewardComponentView.SetQuantity(10);

            Assert.AreEqual("10 remaining", questRewardComponentView.rewardQuantity.text, "Reward quantity text does not match");
        }

        [Test]
        public void SetRewardQuantityOver999()
        {
            questRewardComponentView.SetQuantity(10000);

            Assert.AreEqual(">999 remaining", questRewardComponentView.rewardQuantity.text, "Reward quantity text does not match");
        }
    }
}
