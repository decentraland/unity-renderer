using DCL;
using DCL.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EmotesCustomization
{
    public interface IEmotesCustomizationComponentController : IDisposable
    {
        /// <summary>
        /// Initializes the emotes customization controller.
        /// </summary>
        /// <param name="userProfile">User profile.</param>
        /// <param name="catalog">Wearables catalog.</param>
        void Initialize(UserProfile userProfile, BaseDictionary<string, WearableItem> catalog);
    }

    public class EmotesCustomizationComponentController : IEmotesCustomizationComponentController
    {
        internal const int NUMBER_OF_SLOTS = 10;
        internal const string PLAYER_PREFS_EQUIPPED_EMOTES_KEY = "EquippedNFTEmotes";

        internal BaseVariable<Transform> isInitialized => DataStore.i.emotesCustomization.isInitialized;
        internal BaseCollection<string> equippedEmotes => DataStore.i.emotesCustomization.equippedEmotes;
        internal BaseCollection<string> currentLoadedEmotes => DataStore.i.emotesCustomization.currentLoadedEmotes;
        internal BaseVariable<bool> isStarMenuOpen => DataStore.i.exploreV2.isOpen;
        internal bool isEmotesCustomizationSectionOpen => isStarMenuOpen.Get() && view.isActive;
        internal BaseVariable<bool> avatarEditorVisible => DataStore.i.HUDs.avatarEditorVisible;

        internal IEmotesCustomizationComponentView view;
        internal InputAction_Hold equipInputAction;
        internal InputAction_Hold showInfoInputAction;
        internal InputAction_Trigger shortcut0InputAction;
        internal InputAction_Trigger shortcut1InputAction;
        internal InputAction_Trigger shortcut2InputAction;
        internal InputAction_Trigger shortcut3InputAction;
        internal InputAction_Trigger shortcut4InputAction;
        internal InputAction_Trigger shortcut5InputAction;
        internal InputAction_Trigger shortcut6InputAction;
        internal InputAction_Trigger shortcut7InputAction;
        internal InputAction_Trigger shortcut8InputAction;
        internal InputAction_Trigger shortcut9InputAction;
        internal UserProfile userProfile;
        internal BaseDictionary<string, WearableItem> catalog;

        public void Initialize(UserProfile userProfile, BaseDictionary<string, WearableItem> catalog)
        {
            LoadEquippedEmotes();
            ConfigureView();
            ConfigureCatalog(catalog);
            ConfigureUserProfile(userProfile);
            ConfigureShortcuts();

            isInitialized.Set(view.viewTransform);
        }

        public void Dispose()
        {
            view.onEmoteClicked -= OnEmoteAnimationRaised;
            view.onEmoteEquipped -= OnEmoteEquipped;
            view.onEmoteUnequipped -= OnEmoteUnequipped;
            isStarMenuOpen.OnChange -= IsStarMenuOpenChanged;
            avatarEditorVisible.OnChange -= OnAvatarEditorVisibleChanged;
            catalog.OnAdded -= AddEmote;
            catalog.OnRemoved -= RemoveEmote;
            userProfile.OnInventorySet -= OnUserProfileInventorySet;
            userProfile.OnUpdate -= OnUserProfileUpdated;
            equipInputAction.OnFinished -= OnEquipInputActionTriggered;
            showInfoInputAction.OnFinished -= OnShowInfoInputActionTriggered;
            shortcut0InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut1InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut2InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut3InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut4InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut5InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut6InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut7InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut8InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
            shortcut9InputAction.OnTriggered -= OnNumericShortcutInputActionTriggered;
        }

        internal void LoadEquippedEmotes()
        {
            List<string> storedEquippedEmotes = JsonConvert.DeserializeObject<List<string>>(PlayerPrefsUtils.GetString(PLAYER_PREFS_EQUIPPED_EMOTES_KEY));
            if (storedEquippedEmotes != null)
                equippedEmotes.Set(storedEquippedEmotes);
        }

        internal void ConfigureView()
        {
            view = CreateView();
            view.onEmoteClicked += OnEmoteAnimationRaised;
            view.onEmoteEquipped += OnEmoteEquipped;
            view.onEmoteUnequipped += OnEmoteUnequipped;
            isStarMenuOpen.OnChange += IsStarMenuOpenChanged;
            avatarEditorVisible.OnChange += OnAvatarEditorVisibleChanged;
        }

        internal void OnEmoteAnimationRaised(string emoteId)
        {
            Debug.Log("SANTI ---> EMOTE ANIMATION RAISED: " + emoteId);
        }

        internal void IsStarMenuOpenChanged(bool currentIsOpen, bool previousIsOpen)
        {
            view.CloseEmoteInfoPanel();
        }

        internal void OnAvatarEditorVisibleChanged(bool current, bool previous) { view.SetActive(current); }

        internal void ConfigureCatalog(BaseDictionary<string, WearableItem> catalog)
        {
            this.catalog = catalog;
            this.catalog.OnAdded += AddEmote;
            this.catalog.OnRemoved += RemoveEmote;
        }

        internal void ProcessCatalog()
        {
            CleanEmotes();

            using (var iterator = catalog.Get().GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    AddEmote(iterator.Current.Key, iterator.Current.Value);
                }
            }
        }

        internal void CleanEmotes()
        {
            currentLoadedEmotes.Set(new List<string>());
            view.CleanEmotes();
            UpdateEmoteSlots();
        }

        internal void AddEmote(string id, WearableItem wearable)
        {
            if (!wearable.IsEmote() || currentLoadedEmotes.Contains(id))
                return;

            if (!wearable.data.tags.Contains("base-wearable") && userProfile.GetItemAmount(id) == 0)
                return;

            currentLoadedEmotes.Add(id);
            EmoteCardComponentModel emoteToAdd = ParseWearableItemIntoEmoteCardModel(wearable);
            view.AddEmote(emoteToAdd);
            UpdateEmoteSlots();
        }

        internal void RemoveEmote(string id, WearableItem wearable)
        {
            currentLoadedEmotes.Remove(id);
            view.RemoveEmote(id);
            UpdateEmoteSlots();
        }

        internal EmoteCardComponentModel ParseWearableItemIntoEmoteCardModel(WearableItem wearable)
        {
            return new EmoteCardComponentModel
            {
                id = wearable.id,
                name = wearable.GetName(),
                description = wearable.description,
                pictureUri = wearable.ComposeThumbnailUrl(),
                isAssignedInSelectedSlot = false,
                isSelected = false,
                assignedSlot = -1,
                rarity = wearable.rarity,
                isInL2 = wearable.IsInL2()
            };
        }

        internal void ConfigureUserProfile(UserProfile userProfile)
        {
            this.userProfile = userProfile;
            this.userProfile.OnInventorySet += OnUserProfileInventorySet;
            this.userProfile.OnUpdate += OnUserProfileUpdated;
            OnUserProfileUpdated(this.userProfile);
        }

        internal void OnUserProfileUpdated(UserProfile userProfile)
        {
            if (string.IsNullOrEmpty(userProfile.userId))
                return;

            this.userProfile.OnUpdate -= OnUserProfileUpdated;
            ProcessCatalog();
        }

        internal void OnUserProfileInventorySet(Dictionary<string, int> inventory) { ProcessCatalog(); }

        internal void UpdateEmoteSlots()
        {
            for (int i = 0; i < equippedEmotes.Count(); i++)
            {
                if (i > NUMBER_OF_SLOTS)
                    break;

                if (equippedEmotes[i] == null)
                    continue;

                catalog.TryGetValue(equippedEmotes[i], out WearableItem emoteItem);
                if (emoteItem != null && currentLoadedEmotes.Contains(emoteItem.id))
                    view.EquipEmote(emoteItem.id, emoteItem.GetName(), i, false, false);
            }
        }

        internal void StoreEquippedEmotes()
        {
            List<string> newEquippedEmotesList = new List<string> { null, null, null, null, null, null, null, null, null, null };
            foreach (EmoteSlotCardComponentView slot in view.currentSlots)
            {
                if (!string.IsNullOrEmpty(slot.model.emoteId))
                    newEquippedEmotesList[slot.model.slotNumber] = slot.model.emoteId;
            }

            equippedEmotes.Set(newEquippedEmotesList);
            PlayerPrefsUtils.SetString(PLAYER_PREFS_EQUIPPED_EMOTES_KEY, JsonConvert.SerializeObject(newEquippedEmotesList));
            PlayerPrefsUtils.Save();
        }

        internal void OnEmoteEquipped(string emoteId, int slotNumber)
        {
            StoreEquippedEmotes();
        }

        internal void OnEmoteUnequipped(string emoteId, int slotNumber)
        {
            StoreEquippedEmotes();
        }

        internal void ConfigureShortcuts()
        {
            equipInputAction = Resources.Load<InputAction_Hold>("DefaultConfirmAction");
            equipInputAction.OnFinished += OnEquipInputActionTriggered;

            showInfoInputAction = Resources.Load<InputAction_Hold>("ZoomIn");
            showInfoInputAction.OnFinished += OnShowInfoInputActionTriggered;

            shortcut0InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut0");
            shortcut0InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut1InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut1");
            shortcut1InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut2InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut2");
            shortcut2InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut3InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut3");
            shortcut3InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut4InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut4");
            shortcut4InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut5InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut5");
            shortcut5InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut6InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut6");
            shortcut6InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut7InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut7");
            shortcut7InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut8InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut8");
            shortcut8InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;

            shortcut9InputAction = Resources.Load<InputAction_Trigger>("ToggleShortcut9");
            shortcut9InputAction.OnTriggered += OnNumericShortcutInputActionTriggered;
        }

        internal void OnEquipInputActionTriggered(DCLAction_Hold action)
        {
            if (!isEmotesCustomizationSectionOpen || view.selectedCard == null)
                return;

            if (!view.selectedCard.model.isAssignedInSelectedSlot)
            {
                view.EquipEmote(
                    view.selectedCard.model.id,
                    view.selectedCard.model.name,
                    view.selectedSlot);
            }
            else
            {
                view.UnequipEmote(
                    view.selectedCard.model.id,
                    view.selectedSlot);
            }
        }

        internal void OnShowInfoInputActionTriggered(DCLAction_Hold action)
        {
            if (!isEmotesCustomizationSectionOpen || view.selectedCard == null)
                return;

            view.OpenEmoteInfoPanel(
                view.selectedCard.model,
                view.selectedCard.rarityMark.gameObject.activeSelf ? view.selectedCard.rarityMark.color : Color.grey,
                view.selectedCard.emoteInfoAnchor);
        }

        internal void OnNumericShortcutInputActionTriggered(DCLAction_Trigger action)
        {
            if (!isEmotesCustomizationSectionOpen || view.selectedCard == null)
                return;

            switch (action)
            {
                case DCLAction_Trigger.ToggleShortcut0:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 0);
                    break;
                case DCLAction_Trigger.ToggleShortcut1:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 1);
                    break;
                case DCLAction_Trigger.ToggleShortcut2:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 2);
                    break;
                case DCLAction_Trigger.ToggleShortcut3:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 3);
                    break;
                case DCLAction_Trigger.ToggleShortcut4:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 4);
                    break;
                case DCLAction_Trigger.ToggleShortcut5:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 5);
                    break;
                case DCLAction_Trigger.ToggleShortcut6:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 6);
                    break;
                case DCLAction_Trigger.ToggleShortcut7:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 7);
                    break;
                case DCLAction_Trigger.ToggleShortcut8:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 8);
                    break;
                case DCLAction_Trigger.ToggleShortcut9:
                    view.EquipEmote(view.selectedCard.model.id, view.selectedCard.model.name, 9);
                    break;
            }
        }

        internal virtual IEmotesCustomizationComponentView CreateView() => EmotesCustomizationComponentView.Create();
    }
}