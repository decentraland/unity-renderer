using System;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public interface IWearableGridView
    {
        event Action<int> OnWearablePageChanged;
        event Action<WearableGridItemModel> OnWearableSelected;
        event Action<WearableGridItemModel, EquipWearableSource> OnWearableEquipped;
        event Action<WearableGridItemModel, UnequipWearableSource> OnWearableUnequipped;
        event Action<string> OnFilterSelected;
        event Action<string> OnFilterRemoved;
        event Action OnGoToMarketplace;

        void Dispose();
        void SetWearablePages(int pageNumber, int totalPages);
        void ShowWearables(IEnumerable<WearableGridItemModel> wearables);
        void ClearWearables();
        void SetWearable(WearableGridItemModel model);
        void ClearWearableSelection();
        void SelectWearable(string wearableId);
        void SetWearableBreadcrumb(NftBreadcrumbModel model);
        void FillInfoCard(InfoCardComponentModel model);
        void SetInfoCardVisible(bool isVisible);
        void SetLoadingActive(bool isActive);
        void RefreshWearable(string wearableId);
        void RefreshAllWearables();
    }
}
