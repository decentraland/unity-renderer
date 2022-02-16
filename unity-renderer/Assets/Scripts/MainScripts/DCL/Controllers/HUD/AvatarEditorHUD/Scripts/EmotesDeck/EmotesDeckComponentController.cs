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

        public void Initialize()
        {
            view = CreateView();
            view.onEmoteSelected += OnEmoteSelected;
            view.onEmoteEquipped += OnEmoteEquipped;

            equipInputAction = Resources.Load<InputAction_Hold>("DefaultConfirmAction");
            equipInputAction.OnFinished += OnEquipInputActionTriggered;

            favoriteInputAction = Resources.Load<InputAction_Hold>("DefaultCancelAction");
            favoriteInputAction.OnFinished += OnFavoriteInputActionTriggered;

            LoadMockedEmotes();

            isInitialized.Set(view.emotesDeckTransform);
        }

        public void Dispose()
        {
            view.onEmoteSelected -= OnEmoteSelected;
            view.onEmoteEquipped -= OnEmoteEquipped;
            equipInputAction.OnFinished -= OnEquipInputActionTriggered;
            favoriteInputAction.OnFinished -= OnFavoriteInputActionTriggered;
        }

        internal void OnEmoteSelected(string emoteId)
        {
            Debug.Log("SANTI ---> EMOTE SELECTED: " + emoteId);
        }

        internal void OnEmoteEquipped(string emoteId, int slotNUmber)
        {
            Debug.Log("SANTI ---> EMOTE EQUIPPED: " + emoteId + " | SLOT: " + slotNUmber);
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