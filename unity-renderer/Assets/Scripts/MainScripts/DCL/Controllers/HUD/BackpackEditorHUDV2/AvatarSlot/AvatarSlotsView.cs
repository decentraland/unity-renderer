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

        public event Action<string, bool> OnToggleAvatarSlot;

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
            avatarSlot.SetNftImage("");
            avatarSlot.OnSelectAvatarSlot += (slotCat, isToggled)=>OnToggleAvatarSlot?.Invoke(slotCat, isToggled);
        }

        public void DisablePreviousSlot(string category)
        {
            avatarSlots[category].OnPointerClickOnDifferentSlot();
        }

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
