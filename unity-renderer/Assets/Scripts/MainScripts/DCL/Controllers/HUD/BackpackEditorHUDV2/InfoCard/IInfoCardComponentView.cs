using System.Collections.Generic;

namespace DCL.Backpack
{
    public interface IInfoCardComponentView
    {
        void SetName(string name);

        void SetDescription(string description);

        void SetCategory(string category);

        void SetHidesList(List<string> hideList);

        void SetRemovesList(List<string> removeList);
    }
}
