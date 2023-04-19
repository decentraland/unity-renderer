using System;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public interface IInfoCardComponentView
    {
        event Action<string> OnEquipWearable;
        event Action<string> OnUnEquipWearable;
        event Action OnViewMore;

        void SetName(string name);

        void SetDescription(string description);

        void SetCategory(string category);

        void SetHidesList(List<string> hideList);

        void SetRemovesList(List<string> removeList);

        void SetIsEquipped(bool isEquipped);
    }
}
