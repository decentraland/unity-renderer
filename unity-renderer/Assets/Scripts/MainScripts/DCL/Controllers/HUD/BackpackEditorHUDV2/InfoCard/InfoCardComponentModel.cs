using System;
using System.Collections.Generic;

namespace DCL.Backpack
{
    [Serializable]
    public record InfoCardComponentModel
    {
        public string name;
        public string description;
        public string category;
        public string rarity;
        public string hiddenBy;
        public bool isEquipped;
        public string imageUri;
        public List<string> hideList;
        public List<string> removeList;
        public string wearableId;
        public bool unEquipAllowed = true;
    }
}
