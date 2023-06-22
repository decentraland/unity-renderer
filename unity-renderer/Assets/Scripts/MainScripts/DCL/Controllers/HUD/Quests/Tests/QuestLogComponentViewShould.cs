using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Quests
{
    public class QuestLogComponentViewShould
    {
        private QuestLogComponentView questOfferComponentView;

        [SetUp]
        public void SetUp()
        {
            questOfferComponentView = Object.Instantiate(
                                                 AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/MainScripts/DCL/Controllers/HUD/Quest_V2/QuestLogHUD.prefab"))
                                            .GetComponent<QuestLogComponentView>();
        }

        [TearDown]
        public void TearDown()
        {
            questOfferComponentView.Dispose();
            Object.Destroy(questOfferComponentView);
        }
    }
}
