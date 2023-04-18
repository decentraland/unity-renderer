using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public interface IBackpackEditorHUDView
    {
        event Action<int> OnWearablePageChanged;
        event Action<WearableGridItemModel> OnWearableSelected;
        event Action<WearableGridItemModel> OnWearableEquipped;
        event Action<WearableGridItemModel> OnWearableUnequipped;

        void Dispose();
        void Show();
        void Hide();
        void SetAsFullScreenMenuMode(Transform parentTransform);
        void SetWearablePages(int currentPage, int totalPages);
        void ShowWearables(IEnumerable<WearableGridItemModel> wearables);
        void ClearWearables();
        void ShowEmotes(IEnumerable<EmoteGridItemModel> emotes);
        void ClearEmotes();
        void SetWearable(WearableGridItemModel model);
        void ClearWearableSelection();
        void SelectWearable(string wearableId);
    }
}
