using DCL;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EmotesDeck
{
    public interface IEmotesDeckComponentView
    {
        /// <summary>
        /// It will be triggered when a slot is selected. It returns the selected slot number and the assigned emote id.
        /// </summary>
        event Action<int, string> onSlotSelected;

        /// <summary>
        /// It will be triggered when an emote card is selected.
        /// </summary>
        event Action<string> onEmoteSelected;

        /// <summary>
        /// It will be triggered when an emote is equipped.
        /// </summary>
        event Action<string> onEmoteEquipped;

        /// <summary>
        /// Get the current selected slot number.
        /// </summary>
        int selectedSlot { get; }

        /// <summary>
        /// Set the emotes grid component with a list of emote cards.
        /// </summary>
        /// <param name="realms">List of emote cards (model) to be loaded.</param>
        void SetEmotes(List<EmoteCardComponentModel> emotes);

        /// <summary>
        /// Add a list of emotes in the emotes grid component.
        /// </summary>
        /// <param name="emotes">List of emote cards (model) to be added.</param>
        void AddEmotes(List<EmoteCardComponentModel> emotes);

        /// <summary>
        /// Set an emote as favorite or not.
        /// </summary>
        /// <param name="emoteId">Emote Id to update.</param>
        /// <param name="isFavorite">True for set it as favorite.</param>
        void SetEmoteAsFavorite(string emoteId, bool isFavorite);

        /// <summary>
        /// Assign an emote into a specific slot.
        /// </summary>
        /// <param name="emoteId">Emote Id to assign.</param>
        /// <param name="slotNumber">Slot number to assign the emote.</param>
        void EquipEmote(string emoteId, int slotNumber);
    }

    public class EmotesDeckComponentView : BaseComponentView, IEmotesDeckComponentView
    {
        internal const string EMOTE_CARDS_POOL_NAME = "EmotesDeck_EmoteCardsPool";
        internal const int EMOTE_CARDS_POOL_PREWARM = 40;
        internal const int DEFAULT_SELECTED_SLOT = 1;

        [Header("Assets References")]
        [SerializeField] internal EmoteCardComponentView emoteCardPrefab;

        [Header("Prefab References")]
        [SerializeField] internal EmoteSlotSelectorComponentView emoteSlotSelector;
        [SerializeField] internal EmoteSlotViewerComponentView emoteSlotViewer;
        [SerializeField] internal GridContainerComponentView emotesGrid;

        public event Action<int, string> onSlotSelected;
        public event Action<string> onEmoteSelected;
        public event Action<string> onEmoteEquipped;

        internal Pool emoteCardsPool;

        public int selectedSlot => emoteSlotSelector.selectedSlot;

        public override void Awake()
        {
            base.Awake();

            emoteSlotSelector.onSlotSelected += OnSlotSelected;

            ConfigureEmotesPool();
        }

        public override void Start()
        {
            base.Start();

            emoteSlotSelector.SelectSlot(DEFAULT_SELECTED_SLOT);
            SetMockedEmotes();
        }

        public override void RefreshControl()
        {
            emoteSlotSelector.RefreshControl();
            emoteSlotViewer.RefreshControl();
            emotesGrid.RefreshControl();
        }

        public override void Dispose()
        {
            base.Dispose();

            emoteSlotSelector.onSlotSelected -= OnSlotSelected;
        }

        public void SetEmotes(List<EmoteCardComponentModel> emotes)
        {
            emotesGrid.ExtractItems();
            emoteCardsPool.ReleaseAll();

            List<BaseComponentView> instantiatedEmotes = new List<BaseComponentView>();
            foreach (EmoteCardComponentModel emotesInfo in emotes)
            {
                EmoteCardComponentView emoteGO = InstantiateAndConfigureEmoteCard(emotesInfo);
                instantiatedEmotes.Add(emoteGO);
            }

            emotesGrid.SetItems(instantiatedEmotes);
        }

        public void AddEmotes(List<EmoteCardComponentModel> emotes)
        {
            List<BaseComponentView> instantiatedEmotes = new List<BaseComponentView>();
            foreach (EmoteCardComponentModel emotesInfo in emotes)
            {
                EmoteCardComponentView emoteGO = InstantiateAndConfigureEmoteCard(emotesInfo);
                instantiatedEmotes.Add(emoteGO);
            }

            foreach (var emote in instantiatedEmotes)
            {
                emotesGrid.AddItem(emote);
            }
        }

        public void SetEmoteAsFavorite(string emoteId, bool isFavorite)
        {
            EmoteCardComponentView emoteCardsToUpdate = GetEmoteCardById(emoteId);

            if (emoteCardsToUpdate != null)
                emoteCardsToUpdate.SetEmoteAsFavorite(isFavorite);
        }

        public void EquipEmote(string emoteId, int slotNumber)
        {
            List<EmoteCardComponentView> currentEmoteCards = GetAllEmoteCards();
            foreach (var existingEmoteCard in currentEmoteCards)
            {
                if (existingEmoteCard.model.assignedSlot == slotNumber)
                    existingEmoteCard.AssignSlot(-1);

                if (existingEmoteCard.model.id == emoteId)
                {
                    existingEmoteCard.AssignSlot(slotNumber);
                    emoteSlotSelector.AssignEmoteIntoSlot(slotNumber, emoteId, existingEmoteCard.model.pictureSprite);
                }

                existingEmoteCard.SetEmoteAsAssignedInSelectedSlot(existingEmoteCard.model.assignedSlot == slotNumber);
            }
        }

        internal void ConfigureEmotesPool()
        {
            emoteCardsPool = PoolManager.i.GetPool(EMOTE_CARDS_POOL_NAME);
            if (emoteCardsPool == null)
            {
                emoteCardsPool = PoolManager.i.AddPool(
                    EMOTE_CARDS_POOL_NAME,
                    GameObject.Instantiate(emoteCardPrefab).gameObject,
                    maxPrewarmCount: EMOTE_CARDS_POOL_PREWARM,
                    isPersistent: true);

                emoteCardsPool.ForcePrewarm();
            }
        }

        internal EmoteCardComponentView InstantiateAndConfigureEmoteCard(EmoteCardComponentModel emotesInfo)
        {
            EmoteCardComponentView emoteGO = emoteCardsPool.Get().gameObject.GetComponent<EmoteCardComponentView>();
            emoteGO.Configure(emotesInfo);
            emoteGO.onMainClick.RemoveAllListeners();
            emoteGO.onMainClick.AddListener(() => OnEmoteSelected(emoteGO.model.id));
            emoteGO.onEquipClick.RemoveAllListeners();
            emoteGO.onEquipClick.AddListener(() => OnEmoteEquipped(emoteGO.model.id));

            return emoteGO;
        }

        internal void OnSlotSelected(int slotNumber, string emoteId)
        {
            List<EmoteCardComponentView> currentEmoteCards = GetAllEmoteCards();
            foreach (var existingEmoteCard in currentEmoteCards)
            {
                existingEmoteCard.SetEmoteAsAssignedInSelectedSlot(existingEmoteCard.model.assignedSlot == slotNumber);
            }

            emoteSlotViewer.SetSelectedSlot(slotNumber);
            onSlotSelected?.Invoke(slotNumber, emoteId);
        }

        internal void OnEmoteSelected(string emoteId)
        {
            List<EmoteCardComponentView> currentEmoteCards = GetAllEmoteCards();
            foreach (var existingEmoteCard in currentEmoteCards)
            {
                existingEmoteCard.SetEmoteAsSelected(existingEmoteCard.model.id == emoteId);
            }

            onEmoteSelected?.Invoke(emoteId);
        }

        internal void OnEmoteEquipped(string emoteId)
        {
            EquipEmote(emoteId, selectedSlot);
            onEmoteEquipped?.Invoke(emoteId);
        }

        internal List<EmoteCardComponentView> GetAllEmoteCards()
        {
            return emotesGrid
                .GetItems()
                .Select(x => x as EmoteCardComponentView)
                .ToList();
        }

        internal EmoteCardComponentView GetEmoteCardById(string emoteId)
        {
            return GetAllEmoteCards().FirstOrDefault(x => x.model.id == emoteId);
        }

        // ------------- DEBUG ------------------------
        [ContextMenu("SetMockedEmotes")]
        public void SetMockedEmotes()
        {
            List<EmoteCardComponentModel> mockedEmotes = new List<EmoteCardComponentModel>();

            for (int i = 0; i < 42; i++)
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

            SetEmotes(mockedEmotes);
        }
    }
}