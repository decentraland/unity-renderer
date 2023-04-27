using DCL.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public class AvatarSlotsView : BaseComponentView, IAvatarSlotsView
    {
        [SerializeField] private RectTransform avatarSlotsContainer;

        [Header("Prefab references")]
        [SerializeField] private GameObject avatarSlotSectionPrefab;
        [SerializeField] private GameObject avatarSlotPrefab;
        [SerializeField] private GameObject sectionSeparator;

        private readonly Dictionary<string, Transform> avatarSlotSections = new ();
        private readonly Dictionary<string, IAvatarSlotComponentView> avatarSlots = new ();

        public event IAvatarSlotsView.ToggleAvatarSlotDelegate OnToggleAvatarSlot;
        public event Action<string> OnUnequipFromSlot;

        public void CreateAvatarSlotSection(string sectionName, bool addSeparator)
        {
            avatarSlotSections.Add(sectionName, Instantiate(avatarSlotSectionPrefab, avatarSlotsContainer).transform);

            if (addSeparator)
                Instantiate(sectionSeparator, avatarSlotsContainer);
        }

        public void RebuildLayout()
        {
            // Needed because adding elements to a transform that contains a
            // layout does not refresh the child placements
            Utils.ForceRebuildLayoutImmediate(avatarSlotsContainer);
        }

        public void AddSlotToSection(string sectionName, string slotCategory)
        {
            IAvatarSlotComponentView avatarSlot = Instantiate(avatarSlotPrefab, avatarSlotSections[sectionName]).GetComponent<IAvatarSlotComponentView>();
            avatarSlot.SetCategory(slotCategory);
            avatarSlots.Add(slotCategory, avatarSlot);
            avatarSlot.OnSelectAvatarSlot += (slotModel, isToggled) => OnToggleAvatarSlot?.Invoke(slotModel.category, slotModel.allowsColorChange, isToggled);
            avatarSlot.OnUnEquip += (wearableId) => OnUnequipFromSlot?.Invoke(wearableId);
        }

        public void SetSlotNftImage(string category, string imageUrl) =>
            avatarSlots[category].SetNftImage(imageUrl);

        public void SetSlotRarity(string category, string rarity) =>
            avatarSlots[category].SetRarity(rarity);

        public void DisablePreviousSlot(string category) =>
            avatarSlots[category].OnPointerClickOnDifferentSlot();

        public void ResetCategorySlot(string category) =>
            avatarSlots[category].ResetSlot();

        public void SetWearableId(string category, string wearableId) =>
            avatarSlots[category].SetWearableId(wearableId);

        public void SetSlotsAsHidden(string[] slotsToHide, string hiddenBy)
        {
            foreach (string slotToHide in slotsToHide)
                if (avatarSlots.ContainsKey(slotToHide))
                    avatarSlots[slotToHide].SetIsHidden(true, hiddenBy);
        }

        public override void RefreshControl()
        {
        }
    }
}
