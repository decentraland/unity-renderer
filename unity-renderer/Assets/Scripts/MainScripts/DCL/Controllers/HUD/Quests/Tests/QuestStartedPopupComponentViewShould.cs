using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace DCL.Quests
{
    public class QuestStartedPopupComponentViewShould
    {
        private QuestStartedPopupComponentView questStartedPopupComponentView;

        [SetUp]
        public void SetUp()
        {
            questStartedPopupComponentView = Object.Instantiate(
                                                 AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/MainScripts/DCL/Controllers/HUD/Quests/QuestOfferHUD/QuestStartedPopupHUD.prefab"))
                                            .GetComponent<QuestStartedPopupComponentView>();
        }

        [TearDown]
        public void TearDown()
        {
            questStartedPopupComponentView.Dispose();
            Object.Destroy(questStartedPopupComponentView);
        }

        [Test]
        public void SetQuestName()
        {
            questStartedPopupComponentView.SetQuestName("test name");

            Assert.AreEqual("test name", questStartedPopupComponentView.questNameText.text, "Quest name text does not match");
        }

        [Test]
        public void InvokeOnOpenQuestLog()
        {
            bool invoked = false;
            questStartedPopupComponentView.OnOpenQuestLog += () => invoked = true;

            questStartedPopupComponentView.openQuestLogButton.onClick.Invoke();

            Assert.IsTrue(invoked, "OnOpenQuestLog was not invoked");
        }

        [Test]
        public void SetVisibility()
        {
            questStartedPopupComponentView.SetVisible(true);
            Assert.True(questStartedPopupComponentView.visible, "Not correctly set to visible");

            questStartedPopupComponentView.SetVisible(false);
            Assert.False(questStartedPopupComponentView.visible, "Not correctly set to invisible");
        }
    }
}
