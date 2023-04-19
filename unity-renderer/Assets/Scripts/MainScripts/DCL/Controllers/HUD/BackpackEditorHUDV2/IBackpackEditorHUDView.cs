using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public interface IBackpackEditorHUDView
    {
        Transform EmotesSectionTransform { get; }
        bool isVisible { get; }

        event Action<int> OnWearablePageChanged;
        event Action<WearableGridItemModel> OnWearableSelected;
        event Action<WearableGridItemModel> OnWearableEquipped;
        event Action<WearableGridItemModel> OnWearableUnequipped;
        event Action<string> OnFilterWearables;

        void Initialize(ICharacterPreviewFactory characterPreviewFactory);
        void Dispose();
        void Show(bool instant = false);
        void Hide(bool instant = false);
        void SetAsFullScreenMenuMode(Transform parentTransform);
        void PlayPreviewEmote(string emoteId);
        void ResetPreviewEmote();
        void UpdateAvatarPreview(AvatarModel avatarModel);
        void SetWearablePages(int currentPage, int totalPages);
        void ShowWearables(IEnumerable<WearableGridItemModel> wearables);
        void ClearWearables();
        void ShowEmotes(IEnumerable<EmoteGridItemModel> emotes);
        void ClearEmotes();
        void SetWearable(WearableGridItemModel model);
        void ClearWearableSelection();
        void SelectWearable(string wearableId);
        void SetWearableBreadcrumb(NftBreadcrumbModel model);
    }
}
