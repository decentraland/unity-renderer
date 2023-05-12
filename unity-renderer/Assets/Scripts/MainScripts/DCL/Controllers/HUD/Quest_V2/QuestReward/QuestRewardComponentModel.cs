using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Quests
{
    [Serializable]
    public record QuestRewardComponentModel
    {
        public string name;
        public int quantity;
        public string type;
        public string rarity;
        public string imageUri;
    }
}
