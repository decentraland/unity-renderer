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

        internal DataStore_EmotesCustomization emotesCustomizationDataStore => DataStore.i.emotesCustomization;
        internal BaseDictionary<(string bodyshapeId, string emoteId), AnimationClip> emoteAnimations => DataStore.i.emotes.animations;
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
        internal BaseDictionary<string, EmoteCardComponentView> emotesInLoadingState = new BaseDictionary<string, EmoteCardComponentView>();

        public void Initialize(UserProfile userProfile, BaseDictionary<string, WearableItem> catalog)
        {
            LoadEquippedEmotes();
            ConfigureView();
            ConfigureUserProfileAndCatalog(userProfile, catalog);
            ConfigureShortcuts();

            emotesCustomizationDataStore.isInitialized.Set(view.viewTransform);
        }

        public void Dispose()
        {
            view.onEmoteEquipped -= OnEmoteEquipped;
            view.onEmoteUnequipped -= OnEmoteUnequipped;
            view.onSlotSelected -= OnSlotSelected;
            isStarMenuOpen.OnChange -= IsStarMenuOpenChanged;
            avatarEditorVisible.OnChange -= OnAvatarEditorVisibleChanged;
            emotesCustomizationDataStore.avatarHasBeenSaved.OnChange -= OnAvatarHasBeenSavedChanged;
            catalog.OnAdded -= AddEmote;
            catalog.OnRemoved -= RemoveEmote;
            emoteAnimations.OnAdded -= OnAnimationAdded;
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
            List<string> storedEquippedEmotes;

            try
            {
                storedEquippedEmotes = JsonConvert.DeserializeObject<List<string>>(PlayerPrefsUtils.GetString(PLAYER_PREFS_EQUIPPED_EMOTES_KEY));
            }
            catch
            {
                storedEquippedEmotes = null;
            }

            if (storedEquippedEmotes == null)
                storedEquippedEmotes = GetDefaultEmotes();

            foreach (string emoteId in storedEquippedEmotes)
            {
                if (string.IsNullOrEmpty(emoteId))
                    continue;

                CatalogController.RequestWearable(emoteId);
            }

            List<EquippedEmoteData> storedEquippedEmotesData = new List<EquippedEmoteData>();
            foreach (string emoteId in storedEquippedEmotes)
            {
                storedEquippedEmotesData.Add(
                    string.IsNullOrEmpty(emoteId) ? null : new EquippedEmoteData { id = emoteId, cachedThumbnail = null });
            }
            emotesCustomizationDataStore.equippedEmotes.Set(storedEquippedEmotesData);
        }

        internal List<string> GetDefaultEmotes()
        {
            return new List<string>
            {
                "handsair",
                "wave",
                "fistpump",
                "dance",
                "raiseHand",
                "clap",
                "money",
                "kiss",
                "headexplode",
                "shrug"
            };
        }

        internal void ConfigureView()
        {
            view = CreateView();
            view.onEmoteEquipped += OnEmoteEquipped;
            view.onEmoteUnequipped += OnEmoteUnequipped;
            view.onSlotSelected += OnSlotSelected;
            isStarMenuOpen.OnChange += IsStarMenuOpenChanged;
            avatarEditorVisible.OnChange += OnAvatarEditorVisibleChanged;
            emotesCustomizationDataStore.avatarHasBeenSaved.OnChange += OnAvatarHasBeenSavedChanged;
        }

        internal void IsStarMenuOpenChanged(bool currentIsOpen, bool previousIsOpen) { view.CloseEmoteInfoPanel(); }

        internal void OnAvatarEditorVisibleChanged(bool current, bool previous) { view.SetActive(current); }

        internal void OnAvatarHasBeenSavedChanged(bool wasAvatarSaved, bool previous)
        {
            if (wasAvatarSaved)
            {
                List<string> emotesIdsToStore = new List<string>();
                foreach (EquippedEmoteData equippedEmoteData in emotesCustomizationDataStore.equippedEmotes.Get())
                {
                    emotesIdsToStore.Add(equippedEmoteData != null ? equippedEmoteData.id : null);
                }

                PlayerPrefsUtils.SetString(PLAYER_PREFS_EQUIPPED_EMOTES_KEY, JsonConvert.SerializeObject(emotesIdsToStore));
                PlayerPrefsUtils.Save();
            }
            else
            {
                LoadEquippedEmotes();
                UpdateEmoteSlots();
            }
        }

        internal void ProcessCatalog()
        {
            emotesCustomizationDataStore.currentLoadedEmotes.Set(new List<string>());
            view.CleanEmotes();

            using (var iterator = catalog.Get().GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    AddEmote(iterator.Current.Key, iterator.Current.Value);
                }
            }
        }

        internal void AddEmote(string id, WearableItem wearable)
        {
            if (!wearable.IsEmote() || emotesCustomizationDataStore.currentLoadedEmotes.Contains(id))
                return;

            if (!wearable.data.tags.Contains("base-wearable") && userProfile.GetItemAmount(id) == 0)
                return;

            emotesCustomizationDataStore.currentLoadedEmotes.Add(id);
            EmoteCardComponentModel emoteToAdd = ParseWearableItemIntoEmoteCardModel(wearable);
            EmoteCardComponentView newEmote = view.AddEmote(emoteToAdd);
            newEmote.SetAsLoading(true);

            if (!emotesInLoadingState.ContainsKey(id))
                emotesInLoadingState.Add(id, newEmote);

            RefreshEmoteLoadingState(id);

            UpdateEmoteSlots();
        }

        internal void RemoveEmote(string id, WearableItem wearable)
        {
            emotesCustomizationDataStore.currentLoadedEmotes.Remove(id);
            view.RemoveEmote(id);
            UpdateEmoteSlots();
        }

        internal void OnAnimationAdded((string bodyshapeId, string emoteId) values, AnimationClip animationClip) { RefreshEmoteLoadingState(values.emoteId); }

        internal void RefreshEmoteLoadingState(string emoteId)
        {
            if (emoteAnimations.ContainsKey((userProfile.avatar.bodyShape, emoteId)))
            {
                emotesInLoadingState.TryGetValue(emoteId, out EmoteCardComponentView emote);
                if (emote != null)
                {
                    emote.SetAsLoading(false);
                    emotesInLoadingState.Remove(emoteId);
                }
            }
        }

        internal EmoteCardComponentModel ParseWearableItemIntoEmoteCardModel(WearableItem wearable)
        {
            return new EmoteCardComponentModel
            {
                id = wearable.id,
                name = wearable.GetName(),
                description = wearable.description,
                pictureUri = wearable.ComposeThumbnailUrl(),
                pictureSprite = wearable.thumbnailSprite,
                isAssignedInSelectedSlot = false,
                isSelected = false,
                assignedSlot = -1,
                rarity = wearable.rarity,
                isInL2 = wearable.IsInL2(),
                isLoading = false,
                isCollectible = wearable.IsCollectible()
            };
        }

        internal void ConfigureUserProfileAndCatalog(UserProfile userProfile, BaseDictionary<string, WearableItem> catalog)
        {
            this.userProfile = userProfile;
            this.catalog = catalog;

            this.userProfile.OnInventorySet += OnUserProfileInventorySet;
            this.userProfile.OnUpdate += OnUserProfileUpdated;

            OnUserProfileUpdated(this.userProfile);
        }

        internal void OnUserProfileUpdated(UserProfile userProfile)
        {
            if (string.IsNullOrEmpty(userProfile.userId))
                return;

            this.userProfile.OnUpdate -= OnUserProfileUpdated;
            catalog.OnAdded += AddEmote;
            catalog.OnRemoved += RemoveEmote;
            emoteAnimations.OnAdded += OnAnimationAdded;

            ProcessCatalog();
        }

        internal void OnUserProfileInventorySet(Dictionary<string, int> inventory) { ProcessCatalog(); }

        internal void UpdateEmoteSlots()
        {
            for (int i = 0; i < emotesCustomizationDataStore.equippedEmotes.Count(); i++)
            {
                if (i > NUMBER_OF_SLOTS)
                    break;

                if (emotesCustomizationDataStore.equippedEmotes[i] == null)
                {
                    EmoteSlotCardComponentView existingEmoteIntoSlot = view.currentSlots.FirstOrDefault(x => x.model.slotNumber == i);
                    if (existingEmoteIntoSlot != null)
                        view.UnequipEmote(existingEmoteIntoSlot.model.emoteId, i, false);

                    continue;
                }

                catalog.TryGetValue(emotesCustomizationDataStore.equippedEmotes[i].id, out WearableItem emoteItem);
                if (emoteItem != null && emotesCustomizationDataStore.currentLoadedEmotes.Contains(emoteItem.id))
                    view.EquipEmote(emoteItem.id, emoteItem.GetName(), i, false, false);
            }
        }

        internal void StoreEquippedEmotes()
        {
            List<EquippedEmoteData> newEquippedEmotesList = new List<EquippedEmoteData> { null, null, null, null, null, null, null, null, null, null };
            foreach (EmoteSlotCardComponentView slot in view.currentSlots)
            {
                if (!string.IsNullOrEmpty(slot.model.emoteId))
                    newEquippedEmotesList[slot.model.slotNumber] = new EquippedEmoteData
                    {
                        id = slot.model.emoteId,
                        cachedThumbnail = slot.model.pictureSprite
                    };
            }

            emotesCustomizationDataStore.equippedEmotes.Set(newEquippedEmotesList);
        }

        internal void OnEmoteEquipped(string emoteId, int slotNumber)
        {
            StoreEquippedEmotes();
            emotesCustomizationDataStore.emoteForPreviewing.Set(emoteId, true);
            emotesCustomizationDataStore.emoteForEquipping.Set(emoteId, true);
        }

        internal void OnEmoteUnequipped(string emoteId, int slotNumber)
        {
            StoreEquippedEmotes();
            emotesCustomizationDataStore.emoteForUnequipping.Set(emoteId, true);
        }

        internal void OnSlotSelected(string emoteId, int slotNumber) { emotesCustomizationDataStore.emoteForPreviewing.Set(emoteId, true); }

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
            if (!isEmotesCustomizationSectionOpen || view.selectedCard == null || view.selectedCard.model.isLoading)
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
            if (!isEmotesCustomizationSectionOpen || view.selectedCard == null || view.selectedCard.model.isLoading)
                return;

            view.OpenEmoteInfoPanel(
                view.selectedCard.model,
                view.selectedCard.rarityMark.gameObject.activeSelf ? view.selectedCard.rarityMark.color : Color.grey,
                view.selectedCard.emoteInfoAnchor);
        }

        internal void OnNumericShortcutInputActionTriggered(DCLAction_Trigger action)
        {
            if (!isEmotesCustomizationSectionOpen || view.selectedCard == null || view.selectedCard.model.isLoading)
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