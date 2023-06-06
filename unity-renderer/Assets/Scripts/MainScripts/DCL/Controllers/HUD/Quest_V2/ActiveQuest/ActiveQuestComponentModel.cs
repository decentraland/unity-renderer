using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Quests
{
    [Serializable]
    public record ActiveQuestComponentModel
    {
        public string questId;
        public string questCreator;
        public string questName;
        public string questImageUri;
        public bool isPinned = false;
        public QuestDetailsComponentModel questModel;
    }
}
