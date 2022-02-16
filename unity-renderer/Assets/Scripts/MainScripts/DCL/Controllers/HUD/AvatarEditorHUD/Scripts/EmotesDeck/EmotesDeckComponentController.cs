using DCL;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EmotesDeck
{
    public interface IEmotesDeckComponentController : IDisposable
    {
        /// <summary>
        /// Initializes the emotes deck controller.
        /// </summary>
        void Initialize();
    }

    public class EmotesDeckComponentController : IEmotesDeckComponentController
    {
        internal BaseVariable<Transform> isInitialized => DataStore.i.emotesDeck.isInitialized;
        internal BaseVariable<bool> isStarMenuOpen => DataStore.i.exploreV2.isOpen;
        internal bool shortcutsCanBeUsed => isStarMenuOpen.Get() && view.isActive;

        internal IEmotesDeckComponentView view;
        internal InputAction_Hold equipInputAction;
        internal InputAction_Hold favoriteInputAction;
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

        public void Initialize()
        {
            view = CreateView();
            view.onEmoteSelected += OnEmoteSelected;
            view.onEmoteEquipped += OnEmoteEquipped;
            
            ConfigureShortcuts();
            LoadMockedEmotes();

            isInitialized.Set(view.emotesDeckTransform);
        }

        public void Dispose()
        {
            view.onEmoteSelected -= OnEmoteSelected;
            view.onEmoteEquipped -= OnEmoteEquipped;
            equipInputAction.OnFinished -= OnEquipInputActionTriggered;
            favoriteInputAction.OnFinished -= OnFavoriteInputActionTriggered;
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

        internal void OnEmoteSelected(string emoteId)
        {
            Debug.Log("SANTI ---> EMOTE SELECTED: " + emoteId);
        }

        internal void OnEmoteEquipped(string emoteId, int slotNUmber)
        {
            Debug.Log("SANTI ---> EMOTE EQUIPPED: " + emoteId + " | SLOT: " + slotNUmber);
        }

        internal void ConfigureShortcuts()
        {
            equipInputAction = Resources.Load<InputAction_Hold>("DefaultConfirmAction");
            equipInputAction.OnFinished += OnEquipInputActionTriggered;

            favoriteInputAction = Resources.Load<InputAction_Hold>("DefaultCancelAction");
            favoriteInputAction.OnFinished += OnFavoriteInputActionTriggered;

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
            if (!shortcutsCanBeUsed || view.selectedCard == null)
                return;

            view.EquipEmote(
                view.selectedCard.model.id, 
                view.selectedSlot);
        }

        internal void OnFavoriteInputActionTriggered(DCLAction_Hold action)
        {
            if (!shortcutsCanBeUsed || view.selectedCard == null)
                return;

            view.SetEmoteAsFavorite(
                view.selectedCard.model.id, 
                !view.selectedCard.model.isFavorite);
        }

        internal void OnNumericShortcutInputActionTriggered(DCLAction_Trigger action)
        {
            if (!shortcutsCanBeUsed || view.selectedCard == null)
                return;

            switch (action)
            {
                case DCLAction_Trigger.ToggleShortcut0:
                    view.EquipEmote(view.selectedCard.model.id, 0);
                    break;
                case DCLAction_Trigger.ToggleShortcut1:
                    view.EquipEmote(view.selectedCard.model.id, 1);
                    break;
                case DCLAction_Trigger.ToggleShortcut2:
                    view.EquipEmote(view.selectedCard.model.id, 2);
                    break;
                case DCLAction_Trigger.ToggleShortcut3:
                    view.EquipEmote(view.selectedCard.model.id, 3);
                    break;
                case DCLAction_Trigger.ToggleShortcut4:
                    view.EquipEmote(view.selectedCard.model.id, 4);
                    break;
                case DCLAction_Trigger.ToggleShortcut5:
                    view.EquipEmote(view.selectedCard.model.id, 5);
                    break;
                case DCLAction_Trigger.ToggleShortcut6:
                    view.EquipEmote(view.selectedCard.model.id, 6);
                    break;
                case DCLAction_Trigger.ToggleShortcut7:
                    view.EquipEmote(view.selectedCard.model.id, 7);
                    break;
                case DCLAction_Trigger.ToggleShortcut8:
                    view.EquipEmote(view.selectedCard.model.id, 8);
                    break;
                case DCLAction_Trigger.ToggleShortcut9:
                    view.EquipEmote(view.selectedCard.model.id, 9);
                    break;
            }
        }

        internal virtual IEmotesDeckComponentView CreateView() => EmotesDeckComponentView.Create();

        // ------------- DEBUG ------------------------
        private void LoadMockedEmotes()
        {
            List<EmoteCardComponentModel> mockedEmotes = new List<EmoteCardComponentModel>();

            for (int i = 0; i < 33; i++)
            {
                mockedEmotes.Add(new EmoteCardComponentModel
                {
                    id = $"Emote{i}",
                    pictureUri = $"https://picsum.photos/100?{i}",
                    isFavorite = false,
                    isAssignedInSelectedSlot = false,
                    isSelected = false,
                    assignedSlot = -1
                });
            }

            view.SetEmotes(mockedEmotes);
        }
    }
}