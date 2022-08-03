using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.EmotesCustomization
{
    public class EmotesCustomizationComponentController : IEmotesCustomizationComponentController
    {
        internal const int NUMBER_OF_SLOTS = 10;

        internal bool isEmotesCustomizationSectionOpen => exploreV2DataStore.isOpen.Get() && view.isActive;

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
        internal readonly DataStore dataStore;
        internal UserProfile userProfile;
        internal BaseDictionary<string, WearableItem> catalog;
        internal BaseDictionary<string, EmoteCardComponentView> emotesInLoadingState = new BaseDictionary<string, EmoteCardComponentView>();

        internal DataStore_EmotesCustomization emotesCustomizationDataStore;
        internal DataStore_Emotes emotesDataStore;
        internal DataStore_ExploreV2 exploreV2DataStore;
        internal DataStore_HUDs hudsDataStore;

        public event Action<string> onEmotePreviewed;
        public event Action<string> onEmoteEquipped;
        public event Action<string> onEmoteUnequipped;
        public event Action<string> onEmoteSell;

        public IEmotesCustomizationComponentView Initialize(
            DataStore_EmotesCustomization emotesCustomizationDataStore, 
            DataStore_Emotes emotesDataStore,
            DataStore_ExploreV2 exploreV2DataStore,
            DataStore_HUDs hudsDataStore,
            UserProfile userProfile, 
            BaseDictionary<string, WearableItem> catalog)
        {
            this.emotesCustomizationDataStore = emotesCustomizationDataStore;
            this.emotesDataStore = emotesDataStore;
            this.exploreV2DataStore = exploreV2DataStore;
            this.hudsDataStore = hudsDataStore;

            IEmotesCustomizationComponentView view = ConfigureView();
            ConfigureUserProfileAndCatalog(userProfile, catalog);
            ConfigureShortcuts();

            emotesCustomizationDataStore.equippedEmotes.OnSet += OnEquippedEmotesSet;
            OnEquippedEmotesSet(emotesCustomizationDataStore.equippedEmotes.Get());

            return view;
        }

        public void RestoreEmoteSlots()
        {
            emotesCustomizationDataStore.unsavedEquippedEmotes.Set(emotesCustomizationDataStore.equippedEmotes.Get());
            UpdateEmoteSlots();
        }

        public void Dispose()
        {
            view.onEmoteEquipped -= OnEmoteEquipped;
            view.onEmoteUnequipped -= OnEmoteUnequipped;
            view.onSellEmoteClicked -= OnSellEmoteClicked;
            view.onSlotSelected -= OnSlotSelected;
            exploreV2DataStore.isOpen.OnChange -= IsStarMenuOpenChanged;
            hudsDataStore.avatarEditorVisible.OnChange -= OnAvatarEditorVisibleChanged;
            emotesCustomizationDataStore.equippedEmotes.OnSet -= OnEquippedEmotesSet;
            catalog.OnAdded -= AddEmote;
            catalog.OnRemoved -= RemoveEmote;
            emotesDataStore.animations.OnAdded -= OnAnimationAdded;
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

        internal void OnEquippedEmotesSet(IEnumerable<EquippedEmoteData> equippedEmotes)
        {
            foreach (EquippedEmoteData equippedEmote in equippedEmotes)
            {
                if (equippedEmote == null || string.IsNullOrEmpty(equippedEmote.id))
                    continue;

                // TODO: We should avoid static calls and create injectable interfaces
                CatalogController.RequestWearable(equippedEmote.id);
            }

            emotesCustomizationDataStore.unsavedEquippedEmotes.Set(equippedEmotes);
            UpdateEmoteSlots();
        }

        internal IEmotesCustomizationComponentView ConfigureView()
        {
            view = CreateView();
            view.onEmoteEquipped += OnEmoteEquipped;
            view.onEmoteUnequipped += OnEmoteUnequipped;
            view.onSellEmoteClicked += OnSellEmoteClicked;
            view.onSlotSelected += OnSlotSelected;
            exploreV2DataStore.isOpen.OnChange += IsStarMenuOpenChanged;
            hudsDataStore.avatarEditorVisible.OnChange += OnAvatarEditorVisibleChanged;

            return view;
        }

        internal void IsStarMenuOpenChanged(bool currentIsOpen, bool previousIsOpen) { view.SetEmoteInfoPanelActive(false); }

        internal void OnAvatarEditorVisibleChanged(bool current, bool previous) { view.SetActive(current); }

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

            if (!wearable.data.tags.Contains(WearableLiterals.Tags.BASE_WEARABLE) && userProfile.GetItemAmount(id) == 0)
                return;

            emotesCustomizationDataStore.currentLoadedEmotes.Add(id);
            EmoteCardComponentModel emoteToAdd = ParseWearableItemIntoEmoteCardModel(wearable);
            EmoteCardComponentView newEmote = view.AddEmote(emoteToAdd);

            if (newEmote != null)
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
            if (emotesDataStore.animations.ContainsKey((userProfile.avatar.bodyShape, emoteId)))
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
            // this.userProfile.OnUpdate += OnUserProfileUpdated;

            OnUserProfileUpdated(this.userProfile);
        }

        internal void OnUserProfileUpdated(UserProfile userProfile)
        {
            if (string.IsNullOrEmpty(userProfile.userId))
                return;

            this.userProfile.OnUpdate -= OnUserProfileUpdated;
            catalog.OnAdded += AddEmote;
            catalog.OnRemoved += RemoveEmote;
            emotesDataStore.animations.OnAdded += OnAnimationAdded;

            ProcessCatalog();
        }

        internal void OnUserProfileInventorySet(Dictionary<string, int> inventory) { ProcessCatalog(); }

        internal void UpdateEmoteSlots()
        {
            for (int i = 0; i < emotesCustomizationDataStore.unsavedEquippedEmotes.Count(); i++)
            {
                if (i > NUMBER_OF_SLOTS)
                    break;

                if (emotesCustomizationDataStore.unsavedEquippedEmotes[i] == null)
                {
                    EmoteSlotCardComponentView existingEmoteIntoSlot = view.GetSlot(i);
                    if (existingEmoteIntoSlot != null)
                        view.UnequipEmote(existingEmoteIntoSlot.model.emoteId, i, false);

                    continue;
                }

                catalog.TryGetValue(emotesCustomizationDataStore.unsavedEquippedEmotes[i].id, out WearableItem emoteItem);
                if (emoteItem != null && emotesCustomizationDataStore.currentLoadedEmotes.Contains(emoteItem.id))
                    view.EquipEmote(emoteItem.id, emoteItem.GetName(), i, false, false);
            }
        }

        internal void StoreEquippedEmotes()
        {
            List<EquippedEmoteData> newEquippedEmotesList = new List<EquippedEmoteData> { null, null, null, null, null, null, null, null, null, null };

            if (view.currentSlots != null)
            {
                foreach (EmoteSlotCardComponentView slot in view.currentSlots)
                {
                    if (!string.IsNullOrEmpty(slot.model.emoteId))
                        newEquippedEmotesList[slot.model.slotNumber] = new EquippedEmoteData
                        {
                            id = slot.model.emoteId,
                            cachedThumbnail = slot.model.pictureSprite
                        };
                }
            }

            emotesCustomizationDataStore.unsavedEquippedEmotes.Set(newEquippedEmotesList);
        }

        internal void OnEmoteEquipped(string emoteId, int slotNumber)
        {
            StoreEquippedEmotes();
            onEmotePreviewed?.Invoke(emoteId);
            onEmoteEquipped?.Invoke(emoteId);
        }

        internal void OnEmoteUnequipped(string emoteId, int slotNumber)
        {
            StoreEquippedEmotes();
            onEmoteUnequipped?.Invoke(emoteId);
        }

        internal void OnSellEmoteClicked(string emoteId) { onEmoteSell?.Invoke(emoteId); }

        internal void OnSlotSelected(string emoteId, int slotNumber) { onEmotePreviewed?.Invoke(emoteId); }

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
            if (!isEmotesCustomizationSectionOpen || view.selectedCard == null || !view.selectedCard.model.isCollectible || view.selectedCard.model.isLoading)
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