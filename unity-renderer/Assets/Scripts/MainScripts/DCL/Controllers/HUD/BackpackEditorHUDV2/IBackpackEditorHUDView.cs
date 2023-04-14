using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public interface IBackpackEditorHUDView
    {
        event Action<int> OnWearablePageChanged;

        void Dispose();
        void Show();
        void Hide();
        void SetAsFullScreenMenuMode(Transform parentTransform);
        void SetWearablePages(int currentPage, int totalPages);
        void ShowWearables(IEnumerable<WearableGridItemModel> wearables);
        void ClearWearables();
    }
}
