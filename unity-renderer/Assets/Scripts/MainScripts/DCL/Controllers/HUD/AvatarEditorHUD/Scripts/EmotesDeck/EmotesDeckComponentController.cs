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

        internal IEmotesDeckComponentView view;

        public void Initialize()
        {
            view = CreateView();
            view.onEmoteSelected += OnEmoteSelected;
            view.onEmoteEquipped += OnEmoteEquipped;

            LoadMockedEmotes();

            isInitialized.Set(view.emotesDeckTransform);
        }

        public void Dispose()
        {
            view.onEmoteSelected -= OnEmoteSelected;
            view.onEmoteEquipped -= OnEmoteEquipped;
        }

        internal void OnEmoteSelected(string emoteId)
        {
            Debug.Log("SANTI ---> EMOTE SELECTED: " + emoteId);
        }

        internal void OnEmoteEquipped(string emoteId)
        {
            Debug.Log("SANTI ---> EMOTE EQUIPPED: " + emoteId);
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