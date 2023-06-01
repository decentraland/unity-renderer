using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Emotes;
using UnityEngine;

namespace DCL.EmotesCustomization
{
    public class EmotesCustomizationComponentController : IEmotesCustomizationComponentController
    {
        internal const int NUMBER_OF_SLOTS = 10;
        internal string bodyShapeId;

        internal DataStore_EmotesCustomization emotesCustomizationDataStore;
        internal DataStore_Emotes emotesDataStore;
        internal BaseDictionary<string, EmoteCardComponentView> emotesInLoadingState = new BaseDictionary<string, EmoteCardComponentView>();
        internal InputAction_Hold equipInputAction;
        internal DataStore_ExploreV2 exploreV2DataStore;
        internal DataStore_HUDs hudsDataStore;
        internal Dictionary<string, WearableItem> ownedEmotes = new Dictionary<string, WearableItem>();
        // internal InputAction_Hold showInfoInputAction;
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

        internal IEmotesCustomizationComponentView view;

        internal bool isEmotesCustomizationSectionOpen => exploreV2DataStore.isOpen.Get() && view.isActive;

        public event Action<string> onEmotePreviewed;
        public event Action<string> onEmoteEquipped;
        public event Action<string> onEmoteUnequipped;
        public event Action<string> onEmoteSell;

        public EmotesCustomizationComponentController() { }

        public EmotesCustomizationComponentController(
            DataStore_EmotesCustomization emotesCustomizationDataStore,
            DataStore_Emotes emotesDataStore,
            DataStore_ExploreV2 exploreV2DataStore,
            DataStore_HUDs hudsDataStore,
            Transform parent,
            string viewPath = "EmotesCustomization/EmotesCustomizationSection")
        {
            Initialize(emotesCustomizationDataStore, emotesDataStore, exploreV2DataStore, hudsDataStore, parent, viewPath);
        }

        internal void Initialize(
            DataStore_EmotesCustomization emotesCustomizationDataStore,
            DataStore_Emotes emotesDataStore,
            DataStore_ExploreV2 exploreV2DataStore,
            DataStore_HUDs hudsDataStore,
            Transform parent,
            string viewPath = "EmotesCustomization/EmotesCustomizationSection")
        {
            this.emotesCustomizationDataStore = emotesCustomizationDataStore;
            this.emotesDataStore = emotesDataStore;
            this.exploreV2DataStore = exploreV2DataStore;
            this.hudsDataStore = hudsDataStore;

            ConfigureView(parent, viewPath);
            ConfigureShortcuts();

            emotesCustomizationDataStore.equippedEmotes.OnSet += OnEquippedEmotesSet;
            OnEquippedEmotesSet(emotesCustomizationDataStore.equippedEmotes.Get());

            emotesDataStore.animations.OnAdded -= OnAnimationAdded;
            emotesDataStore.animations.OnAdded += OnAnimationAdded;
        }

        public void SetEmotes(WearableItem[] ownedEmotes)
        {
            //Find loaded emotes that are not contained in emotesToSet
            List<string> idsToRemove = emotesCustomizationDataStore.currentLoadedEmotes.Get().Where(x => ownedEmotes.All(y => x != y.id)).ToList();

            foreach (string emoteId in idsToRemove)
            {
                RemoveEmote(emoteId);
            }

            this.ownedEmotes.Clear();
            for (int i = 0; i < ownedEmotes.Length; i++)
            {
                WearableItem emote = ownedEmotes[i];
                //emotes owned multiple times will be received as separated entities, overriding by emote.id removes that information
                //if we want to show the amount of ocurrences we can modify this
                this.ownedEmotes[emote.id] = emote;
            }
            foreach (WearableItem emote in this.ownedEmotes.Values)
            {
                AddEmote(emote);
            }
            UpdateEmoteSlots();
        }

        public void SetEquippedBodyShape(string bodyShapeId)
        {
            if (bodyShapeId == this.bodyShapeId)
                return;

            this.bodyShapeId = bodyShapeId;
            foreach (string emoteId in this.ownedEmotes.Keys)
            {
                RefreshEmoteLoadingState(emoteId);
            }
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
            emotesDataStore.animations.OnAdded -= OnAnimationAdded;
            equipInputAction.OnFinished -= OnEquipInputActionTriggered;
            // showInfoInputAction.OnFinished -= OnShowInfoInputActionTriggered;
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
            emotesCustomizationDataStore.unsavedEquippedEmotes.Set(equippedEmotes);
            UpdateEmoteSlots();
        }

        private void ConfigureView(Transform parent, string viewPath)
        {
            view = CreateView(viewPath);
            if (view.viewTransform != null)
                view.viewTransform.SetParent(parent, false);
            view.onEmoteEquipped += OnEmoteEquipped;
            view.onEmoteUnequipped += OnEmoteUnequipped;
            view.onSellEmoteClicked += OnSellEmoteClicked;
            view.onSlotSelected += OnSlotSelected;
            exploreV2DataStore.isOpen.OnChange += IsStarMenuOpenChanged;
            hudsDataStore.avatarEditorVisible.OnChange += OnAvatarEditorVisibleChanged;
        }

        internal void IsStarMenuOpenChanged(bool currentIsOpen, bool previousIsOpen) { view.SetEmoteInfoPanelActive(false); }

        internal void OnAvatarEditorVisibleChanged(bool current, bool previous) { view.SetActive(current); }

        internal void AddEmote(WearableItem emote)
        {
            var emoteId = emote.id;
            if (!emote.IsEmote() || emotesCustomizationDataStore.currentLoadedEmotes.Contains(emoteId))
                return;

            emotesCustomizationDataStore.currentLoadedEmotes.Add(emoteId);
            emotesDataStore.emotesOnUse.IncreaseRefCount((WearableLiterals.BodyShapes.FEMALE, emoteId));
            emotesDataStore.emotesOnUse.IncreaseRefCount((WearableLiterals.BodyShapes.MALE, emoteId));

            if (!emote.ShowInBackpack())
                return;

            EmoteCardComponentModel emoteToAdd = ParseWearableItemIntoEmoteCardModel(emote);
            EmoteCardComponentView newEmote = view.AddEmote(emoteToAdd);

            if (newEmote != null)
                newEmote.SetAsLoading(true);

            if (!emotesInLoadingState.ContainsKey(emoteId))
                emotesInLoadingState.Add(emoteId, newEmote);

            RefreshEmoteLoadingState(emoteId);
        }

        internal void RemoveEmote(string emoteId)
        {
            emotesDataStore.emotesOnUse.DecreaseRefCount((WearableLiterals.BodyShapes.FEMALE, emoteId));
            emotesDataStore.emotesOnUse.DecreaseRefCount((WearableLiterals.BodyShapes.MALE, emoteId));
            emotesCustomizationDataStore.currentLoadedEmotes.Remove(emoteId);
            view.RemoveEmote(emoteId);
            UpdateEmoteSlots();
        }

        internal void OnAnimationAdded((string bodyshapeId, string emoteId) values, EmoteClipData emoteClipData)
        {
            if(emoteClipData.clip != null)
                RefreshEmoteLoadingState(values.emoteId);
            else
            {
                var emoteId = values.emoteId;
                RemoveEmote(emoteId);
                Debug.LogError("Emote " + emoteId + " was not found in emotes data store");
            }
        }

        internal void RefreshEmoteLoadingState(string emoteId)
        {
            if (emotesDataStore.animations.ContainsKey((bodyShapeId, emoteId)))
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

        internal void UpdateEmoteSlots()
        {
            for (int i = 0; i < emotesCustomizationDataStore.unsavedEquippedEmotes.Count(); i++)
            {
                if (i > NUMBER_OF_SLOTS)
                    break;

                if (emotesCustomizationDataStore.unsavedEquippedEmotes[i] == null) //empty slot
                {
                    EmoteSlotCardComponentView existingEmoteIntoSlot = view.GetSlot(i);
                    if (existingEmoteIntoSlot != null)
                        view.UnequipEmote(existingEmoteIntoSlot.model.emoteId, i, false);

                    continue;
                }

                ownedEmotes.TryGetValue(emotesCustomizationDataStore.unsavedEquippedEmotes[i].id, out WearableItem emoteItem);
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

            // showInfoInputAction = Resources.Load<InputAction_Hold>("ZoomIn");
            // showInfoInputAction.OnFinished += OnShowInfoInputActionTriggered;

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

        internal virtual IEmotesCustomizationComponentView CreateView(string path = "EmotesCustomization/EmotesCustomizationSection") =>
            EmotesCustomizationComponentView.Create(path);
    }
}
