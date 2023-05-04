using System;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public interface IWearableGridView
    {
        event Action<int> OnWearablePageChanged;
        event Action<WearableGridItemModel> OnWearableSelected;
        event Action<WearableGridItemModel> OnWearableEquipped;
        event Action<WearableGridItemModel> OnWearableUnequipped;
        event Action<string> OnFilterWearables;
        event Action OnGoToMarketplace;

        void Dispose();
        void SetWearablePages(int totalPages);
        void GoToPage(int pageNumber, bool notifyEvent = true);
        void ShowWearables(IEnumerable<WearableGridItemModel> wearables);
        void ClearWearables();
        void SetWearable(WearableGridItemModel model);
        void ClearWearableSelection();
        void SelectWearable(string wearableId);
        void SetWearableBreadcrumb(NftBreadcrumbModel model);
        void FillInfoCard(InfoCardComponentModel model);
    }
}
