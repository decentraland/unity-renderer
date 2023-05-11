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

        public void AddSlotToSection(string sectionName, string slotCategory, bool allowUnEquip)
        {
            IAvatarSlotComponentView avatarSlot = Instantiate(avatarSlotPrefab, avatarSlotSections[sectionName]).GetComponent<IAvatarSlotComponentView>();
            avatarSlot.SetCategory(slotCategory);
            avatarSlot.SetUnEquipAllowed(allowUnEquip);
            avatarSlots.Add(slotCategory, avatarSlot);
            avatarSlot.OnSelectAvatarSlot += (slotModel, isToggled) => OnToggleAvatarSlot?.Invoke(slotModel.category, slotModel.allowsColorChange, isToggled);
            avatarSlot.OnUnEquip += (wearableId) => OnUnequipFromSlot?.Invoke(wearableId);
            avatarSlot.OnFocusHiddenBy += (hiddenBy) => avatarSlots[hiddenBy].ShakeAnimation();
        }

        public void DisablePreviousSlot(string category) =>
            avatarSlots[category].OnPointerClickOnDifferentSlot();

        public void SetSlotContent(string category, WearableItem wearableItem, string bodyShape)
        {
            avatarSlots[category].SetRarity(wearableItem.rarity);
            avatarSlots[category].SetNftImage(wearableItem.ComposeThumbnailUrl());
            avatarSlots[category].SetWearableId(wearableItem.id);
            avatarSlots[category].SetHideList(wearableItem.GetHidesList(bodyShape));
            RecalculateHideList();
        }

        public void ResetCategorySlot(string category)
        {
            if (avatarSlots[category].GetHideList() != null)
                foreach (var slot in avatarSlots[category].GetHideList())
                    if (avatarSlots.ContainsKey(slot))
                        avatarSlots[slot].SetIsHidden(false, category);

            avatarSlots[category].ResetSlot();
        }

        public void SetWearableId(string category, string wearableId) =>
            avatarSlots[category].SetWearableId(wearableId);

        public void SetSlotsAsHidden(string[] slotsToHide, string hiddenBy)
        {
            foreach (string slotToHide in slotsToHide)
                if (avatarSlots.ContainsKey(slotToHide))
                    avatarSlots[slotToHide].SetIsHidden(true, hiddenBy);
        }
        private static readonly List<string> CATEGORIES_PRIORITY = new ()
        {
            "skin", "upper_body", "lower_body", "feet", "helmet", "hat", "top_head", "mask", "eyewear", "earring", "tiara",
        };
        private void RecalculateHideList()
        {
            HashSet<string> alreadyProcessedCategories = new HashSet<string>();
            foreach (string s in CATEGORIES_PRIORITY)
            {
                if (avatarSlots.ContainsKey(s))
                {
                    foreach (string s1 in avatarSlots[s].GetHideList())
                    {
                        //if it hides a slot that doesn't exist, avoid processing hides
                        if (!avatarSlots.ContainsKey(s1)) continue;
                        //if category has already been processed, avoid processing hides
                        if (alreadyProcessedCategories.Contains(s1)) continue;

                        avatarSlots[s1].SetIsHidden(true, s);
                    }
                    alreadyProcessedCategories.Add(s);
                }
            }
        }

        public override void RefreshControl()
        {
        }
    }
}
