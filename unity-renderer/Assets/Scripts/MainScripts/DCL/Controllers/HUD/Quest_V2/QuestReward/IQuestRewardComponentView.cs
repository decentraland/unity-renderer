using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Quests
{
    public interface IQuestRewardComponentView
    {
        void SetName(string name);
        void SetQuantity(int quantity);
        void SetType(string category);
        void SetRarity(string rarity);
        void SetImage(string imageUri);
    }
}
