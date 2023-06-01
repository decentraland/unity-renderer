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
        public event Action<string, bool> OnHideUnhidePressed;

        private Dictionary<string, HashSet<string>> previouslyHidden;

        public override void Awake()
        {
            previouslyHidden = new Dictionary<string, HashSet<string>>();

            foreach (string category in WearableItem.CATEGORIES_PRIORITY)
                previouslyHidden.Add(category, new HashSet<string>());
        }

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
            avatarSlot.OnSelectAvatarSlot += (slotModel, isToggled) => OnToggleAvatarSlot?.Invoke(slotModel.category, slotModel.allowsColorChange, slotModel.previewCameraFocus, isToggled);
            avatarSlot.OnUnEquip += (wearableId) => OnUnequipFromSlot?.Invoke(wearableId);
            avatarSlot.OnFocusHiddenBy += (hiddenBy) => avatarSlots[hiddenBy].ShakeAnimation();
            avatarSlot.OnHideUnhidePressed += (category, isOverridden) => OnHideUnhidePressed?.Invoke(category, isOverridden);
        }

        public void DisablePreviousSlot(string category) =>
            avatarSlots[category].OnPointerClickOnDifferentSlot();

        public void SetSlotContent(string category, WearableItem wearableItem, string bodyShape, HashSet<string> forceRender)
        {
            avatarSlots[category].SetRarity(wearableItem.rarity);
            avatarSlots[category].SetNftImage(wearableItem.ComposeThumbnailUrl());
            avatarSlots[category].SetWearableId(wearableItem.id);
            avatarSlots[category].SetHideList(wearableItem.GetHidesList(bodyShape));
            RecalculateHideList(forceRender);
        }

        public void ResetCategorySlot(string category, HashSet<string> forceRender)
        {
            if (avatarSlots[category].GetHideList() != null)
                foreach (var slot in avatarSlots[category].GetHideList())
                    if (avatarSlots.ContainsKey(slot))
                        avatarSlots[slot].SetIsHidden(false, category);

            avatarSlots[category].ResetSlot();
            RecalculateHideList(forceRender);
        }

        public void RecalculateHideList(HashSet<string> forceRender)
        {
            foreach (var avatarSlotComponentView in avatarSlots)
            {
                bool isHidden = forceRender.Contains(avatarSlotComponentView.Key);
                avatarSlotComponentView.Value.SetForceRender(isHidden);
            }

            foreach (string category in WearableItem.CATEGORIES_PRIORITY)
                previouslyHidden[category] = new HashSet<string>();

            foreach (string priorityCategory in WearableItem.CATEGORIES_PRIORITY)
            {
                if (!avatarSlots.ContainsKey(priorityCategory) || avatarSlots[priorityCategory].GetHideList() == null)
                    continue;

                foreach (string categoryToHide in avatarSlots[priorityCategory].GetHideList())
                {
                    //if it hides a slot that doesn't exist, avoid processing hides
                    if (!avatarSlots.ContainsKey(categoryToHide))
                        continue;

                    //if category has already been processed, avoid processing hides
                    if (previouslyHidden[categoryToHide].Contains(priorityCategory))
                    {
                        avatarSlots[categoryToHide].SetIsHidden(false, priorityCategory);
                        continue;
                    }

                    previouslyHidden[priorityCategory].Add(categoryToHide);

                    if (forceRender != null && forceRender.Contains(categoryToHide))
                    {
                        avatarSlots[categoryToHide].SetIsHidden(false, priorityCategory);
                        avatarSlots[categoryToHide].SetHideIconVisible(true);
                        continue;
                    }

                    avatarSlots[categoryToHide].SetIsHidden(true, priorityCategory);
                }
            }
        }

        public void SetHideUnhideStatus(string slotCategory, bool isOverridden)
        {
            if (slotCategory == null)
                return;

            if (avatarSlots.TryGetValue(slotCategory, out var slot))
                slot.SetForceRender(isOverridden);
        }

        public void Select(string category, bool notify)
        {
            if (avatarSlots.TryGetValue(category, out var slot))
                slot.Select(notify);
        }

        public void UnSelectAll(bool notify)
        {
            foreach (KeyValuePair<string,IAvatarSlotComponentView> slot in avatarSlots)
                slot.Value.UnSelect(notify);
        }

        public override void RefreshControl() { }
    }
}
