using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DCL.Quests
{
    public class ActiveQuestComponentViewShould
    {
        private ActiveQuestComponentView activeQuestComponentView;

        [SetUp]
        public void SetUp()
        {
            activeQuestComponentView = Object.Instantiate(
                                                     AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/MainScripts/DCL/Controllers/HUD/Quest_V2/ActiveQuest/ActiveQuest.prefab"))
                                                .GetComponent<ActiveQuestComponentView>();
        }

        [TearDown]
        public void TearDown()
        {
            activeQuestComponentView.Dispose();
            Object.Destroy(activeQuestComponentView.gameObject);
        }

        [Test]
        public void SetQuestName()
        {
            activeQuestComponentView.SetQuestName("test title");

            Assert.AreEqual("test title", activeQuestComponentView.questName.text, "Quest title text does not match");
        }

        [Test]
        public void SetQuestCreator()
        {
            activeQuestComponentView.SetQuestCreator("test creator");

            Assert.AreEqual("test creator", activeQuestComponentView.questCreator.text, "Quest creator text does not match");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetIsPinned(bool isPinned)
        {
            activeQuestComponentView.SetIsPinned(isPinned);

            Assert.AreEqual(isPinned, activeQuestComponentView.pinnedIcon.activeInHierarchy, "Pinned icon was not correctly set");
        }

        [Test]
        public void SetIsFocused()
        {
            activeQuestComponentView.OnFocus();

            Assert.True(activeQuestComponentView.focusOutline.activeInHierarchy, "Focus outline was not correctly set");
        }

        [Test]
        public void SetIsNotFocused()
        {
            activeQuestComponentView.OnLoseFocus();

            Assert.False(activeQuestComponentView.focusOutline.activeInHierarchy, "Focus outline was not correctly set");
        }

        [Test]
        public void SetIsSelected()
        {
            string questId = "asdasdads";
            string receivedQuestId = "";
            activeQuestComponentView.SetQuestId(questId);
            activeQuestComponentView.OnActiveQuestSelected += s => receivedQuestId = s;
            activeQuestComponentView.OnPointerClick(null);

            Assert.AreEqual(questId, receivedQuestId, "Even for selected quest was not invoked or was carrying the wrong quest id");
            Assert.True(activeQuestComponentView.isSelected, "is selected was not correctly set");
            Assert.True(activeQuestComponentView.selectedOutline.activeInHierarchy, "selected outline was not correctly set");
            Assert.AreEqual(activeQuestComponentView.selectedNameColor, activeQuestComponentView.questName.color, "Name color not correctly set");
            Assert.AreEqual(activeQuestComponentView.selectedCreatorColor, activeQuestComponentView.questCreator.color, "Creator color not correctly set");
            Assert.AreEqual(activeQuestComponentView.selectedBackgroundColor, activeQuestComponentView.background.color, "Background color not correctly set");
        }

        [Test]
        public void SetIsDeselected()
        {
            activeQuestComponentView.Deselect();

            Assert.False(activeQuestComponentView.isSelected, "is selected was not correctly set");
            Assert.False(activeQuestComponentView.selectedOutline.activeInHierarchy, "selected outline was not correctly set");
            Assert.AreEqual(activeQuestComponentView.deselectedNameColor, activeQuestComponentView.questName.color, "Name color not correctly set");
            Assert.AreEqual(activeQuestComponentView.deselectedCreatorColor, activeQuestComponentView.questCreator.color, "Creator color not correctly set");
            Assert.AreEqual(activeQuestComponentView.deselectedBackgroundColor, activeQuestComponentView.background.color, "Background color not correctly set");
        }
    }
}
