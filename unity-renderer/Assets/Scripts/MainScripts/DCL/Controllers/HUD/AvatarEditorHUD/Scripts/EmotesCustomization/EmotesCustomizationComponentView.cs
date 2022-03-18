using DCL;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EmotesCustomization
{
    public interface IEmotesCustomizationComponentView
    {
        /// <summary>
        /// It will be triggered when an emote is equipped.
        /// </summary>
        event Action<string, int> onEmoteEquipped;

        /// <summary>
        /// It will be triggered when an emote is unequipped.
        /// </summary>
        event Action<string, int> onEmoteUnequipped;

        /// <summary>
        /// It will be triggered when a slot is selected.
        /// </summary>
        event Action<string, int> onSlotSelected;

        /// <summary>
        /// It represents the container transform of the component.
        /// </summary>
        Transform viewTransform { get; }

        /// <summary>
        /// Resturn true if the view is currently active.
        /// </summary>
        bool isActive { get; }

        /// <summary>
        /// Get the current selected slot number.
        /// </summary>
        int selectedSlot { get; }

        /// <summary>
        /// Get the current slots.
        /// </summary>
        List<EmoteSlotCardComponentView> currentSlots { get; }

        /// <summary>
        /// Get the current selected card.
        /// </summary>
        EmoteCardComponentView selectedCard { get; }

        /// <summary>
        /// Clean all the emotes loaded in the grid component.
        /// </summary>
        void CleanEmotes();

        /// <summary>
        /// Add an emote in the emotes grid component.
        /// </summary>
        /// <param name="emote">Emote card (model) to be added.</param>
        EmoteCardComponentView AddEmote(EmoteCardComponentModel emote);

        /// <summary>
        /// Remove an emote from the frid component.
        /// </summary>
        /// <param name="emoteId">Emote id to remove.</param>
        void RemoveEmote(string emoteId);

        /// <summary>
        /// Assign an emote into a specific slot.
        /// </summary>
        /// <param name="emoteId">Emote Id to assign.</param>
        /// <param name="emoteName">Emote name to assign.</param>
        /// <param name="slotNumber">Slot number to assign the emote.</param>
        /// <param name="selectSlotAfterEquip">Indicates if we want to keep selected the asigned slot or not.</param>
        /// <param name="notifyEvent">Indicates if the new equipped emote event should be notified or not.</param>
        void EquipEmote(string emoteId, string emoteName, int slotNumber, bool selectSlotAfterEquip = true, bool notifyEvent = true);

        /// <summary>
        /// Unassign an emote from a specific slot.
        /// </summary>
        /// <param name="emoteId">Emote Id to unasign.</param>
        /// <param name="slotNumber">Slot number to unassign the emote.</param>
        /// <param name="notifyEvent">Indicates if the new equipped emote event should be notified or not.</param>
        void UnequipEmote(string emoteId, int slotNumber, bool notifyEvent = true);

        /// <summary>
        /// Open the info panel for a specific emote.
        /// </summary>
        /// <param name="emoteModel">Model of the emote.</param>
        /// <param name="backgroundColor">Color to apply to the panel background.</param>
        /// <param name="anchorTransform">Anchor where to place the panel.</param>
        void OpenEmoteInfoPanel(EmoteCardComponentModel emoteModel, Color backgroundColor, Transform anchorTransform);

        /// <summary>
        /// Close the info panel.
        /// </summary>
        void CloseEmoteInfoPanel();

        /// <summary>
        /// Get an emote card by id.
        /// </summary>
        /// <param name="emoteId">Emote id to search.</param>
        /// <returns>An emote card.</returns>
        EmoteCardComponentView GetEmoteCardById(string emoteId);

        /// <summary>
        /// Set the view as active or not.
        /// </summary>
        /// <param name="isActive">True for activating it.</param>
        void SetActive(bool isActive);
    }

    public class EmotesCustomizationComponentView : BaseComponentView, IEmotesCustomizationComponentView
    {
        internal const string EMOTE_CARDS_POOL_NAME = "EmotesCustomization_EmoteCardsPool";
        internal const int EMOTE_CARDS_POOL_PREWARM = 40;
        internal const int DEFAULT_SELECTED_SLOT = 1;

        [Header("Assets References")]
        [SerializeField] internal EmoteCardComponentView emoteCardPrefab;

        [Header("Prefab References")]
        [SerializeField] internal EmoteSlotSelectorComponentView emoteSlotSelector;
        [SerializeField] internal GridContainerComponentView emotesGrid;
        [SerializeField] internal NFTItemInfo emoteInfoPanel;

        public event Action<string, int> onEmoteEquipped;
        public event Action<string, int> onEmoteUnequipped;
        public event Action<string, int> onSlotSelected;

        internal Pool emoteCardsPool;

        public bool isActive => gameObject.activeInHierarchy;
        public Transform viewTransform => transform;
        public int selectedSlot => emoteSlotSelector.selectedSlot;
        public List<EmoteSlotCardComponentView> currentSlots => emoteSlotSelector.GetAllSlots();
        public EmoteCardComponentView selectedCard { get; private set; }

        public override void Awake()
        {
            base.Awake();

            emoteSlotSelector.onSlotSelected += OnSlotSelected;

            ConfigureEmotesPool();
            ConfigureEmoteInfoPanel();
        }

        public override void Start()
        {
            base.Start();

            emoteSlotSelector.SelectSlot(DEFAULT_SELECTED_SLOT);
        }

        public override void RefreshControl()
        {
            emoteSlotSelector.RefreshControl();
            emotesGrid.RefreshControl();
        }

        public override void Dispose()
        {
            base.Dispose();

            emoteSlotSelector.onSlotSelected -= OnSlotSelected;
            emoteInfoPanel.closeButton.onClick.RemoveAllListeners();
            emoteInfoPanel.sellButton.onClick.RemoveAllListeners();
        }

        public void CleanEmotes()
        {
            emotesGrid.ExtractItems();
            emoteCardsPool.ReleaseAll();
        }

        public EmoteCardComponentView AddEmote(EmoteCardComponentModel emote)
        {
            EmoteCardComponentView emoteGO = InstantiateAndConfigureEmoteCard(emote);
            emotesGrid.AddItem(emoteGO);

            return emoteGO;
        }

        public void RemoveEmote(string emoteId)
        {
            EmoteCardComponentView emoteToRemove = GetEmoteCardById(emoteId);
            if (emoteToRemove != null)
                emotesGrid.RemoveItem(emoteToRemove);
        }

        public void EquipEmote(string emoteId, string emoteName, int slotNumber, bool selectSlotAfterEquip = true, bool notifyEvent = true)
        {
            if (string.IsNullOrEmpty(emoteId))
                return;

            EmoteCardComponentView emoteCardsToUpdate = GetEmoteCardById(emoteId);
            if (emoteCardsToUpdate != null && emoteCardsToUpdate.model.assignedSlot == slotNumber)
                return;

            List<EmoteCardComponentView> currentEmoteCards = GetAllEmoteCards();
            foreach (var existingEmoteCard in currentEmoteCards)
            {
                if (existingEmoteCard.model.assignedSlot == slotNumber)
                    existingEmoteCard.AssignSlot(-1);

                if (existingEmoteCard.model.id == emoteId)
                {
                    existingEmoteCard.AssignSlot(slotNumber);
                    emoteSlotSelector.AssignEmoteIntoSlot(
                        slotNumber, 
                        emoteId, 
                        emoteName, 
                        existingEmoteCard.model.pictureSprite, 
                        existingEmoteCard.model.pictureUri,
                        existingEmoteCard.model.rarity);

                    if (selectSlotAfterEquip)
                        emoteSlotSelector.SelectSlot(slotNumber);
                }

                existingEmoteCard.SetEmoteAsAssignedInSelectedSlot(existingEmoteCard.model.assignedSlot == selectedSlot);
            }

            emoteInfoPanel.SetActive(false);

            if (notifyEvent)
                onEmoteEquipped?.Invoke(emoteId, slotNumber);
        }

        public void UnequipEmote(string emoteId, int slotNumber, bool notifyEvent = true)
        {
            if (string.IsNullOrEmpty(emoteId))
                return;

            EmoteCardComponentView emoteCardsToUpdate = GetEmoteCardById(emoteId);
            if (emoteCardsToUpdate != null)
            {
                emoteCardsToUpdate.AssignSlot(-1);
                emoteCardsToUpdate.SetEmoteAsAssignedInSelectedSlot(false);
            }

            emoteSlotSelector.AssignEmoteIntoSlot(
                slotNumber, 
                string.Empty, 
                string.Empty, 
                null,
                null,
                string.Empty);
            emoteInfoPanel.SetActive(false);

            if (notifyEvent)
                onEmoteUnequipped?.Invoke(emoteId, slotNumber);
        }

        public void OpenEmoteInfoPanel(EmoteCardComponentModel emoteModel, Color backgroundColor, Transform anchorTransform)
        {
            emoteInfoPanel.SetModel(NFTItemInfo.Model.FromEmoteItem(emoteModel));
            emoteInfoPanel.SetBackgroundColor(backgroundColor);
            emoteInfoPanel.SetRarityName(emoteModel.rarity);
            emoteInfoPanel.SetActive(true);
            emoteInfoPanel.transform.SetParent(anchorTransform);
            emoteInfoPanel.transform.localPosition = Vector3.zero;
        }

        public void CloseEmoteInfoPanel() { emoteInfoPanel.SetActive(false); }

        public EmoteCardComponentView GetEmoteCardById(string emoteId) { return GetAllEmoteCards().FirstOrDefault(x => x.model.id == emoteId); }

        public void SetActive(bool isActive) { gameObject.SetActive(isActive); }

        internal void ClickOnEmote(string emoteId, string emoteName, int slotNumber, bool isAssignedInSelectedSlot)
        {
            if (!isAssignedInSelectedSlot)
                EquipEmote(emoteId, emoteName, slotNumber);
            else
                UnequipEmote(emoteId, selectedSlot);

            emoteInfoPanel.SetActive(false);
        }

        internal void OnEmoteSelected(string emoteId) { selectedCard = GetEmoteCardById(emoteId); }

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

        internal void ConfigureEmoteInfoPanel()
        {
            emoteInfoPanel.closeButton.onClick.AddListener(() => emoteInfoPanel.SetActive(false));
            emoteInfoPanel.sellButton.onClick.AddListener(() =>
            {
                // TODO (Santi): GO TO SELL LINK...
            });
        }

        internal EmoteCardComponentView InstantiateAndConfigureEmoteCard(EmoteCardComponentModel emotesInfo)
        {
            EmoteCardComponentView emoteGO = emoteCardsPool.Get().gameObject.GetComponent<EmoteCardComponentView>();
            emoteGO.Configure(emotesInfo);
            emoteGO.onMainClick.RemoveAllListeners();
            emoteGO.onMainClick.AddListener(() => ClickOnEmote(emoteGO.model.id, emoteGO.model.name, selectedSlot, emoteGO.model.isAssignedInSelectedSlot));
            emoteGO.onInfoClick.RemoveAllListeners();
            emoteGO.onInfoClick.AddListener(() => OpenEmoteInfoPanel(
                emoteGO.model,
                emoteGO.rarityMark.gameObject.activeSelf ? emoteGO.rarityMark.color : Color.grey, 
                emoteGO.emoteInfoAnchor));
            emoteGO.onEmoteSelected -= OnEmoteSelected;
            emoteGO.onEmoteSelected += OnEmoteSelected;

            return emoteGO;
        }

        internal void OnSlotSelected(int slotNumber, string emoteId)
        {
            List<EmoteCardComponentView> currentEmoteCards = GetAllEmoteCards();
            foreach (var existingEmoteCard in currentEmoteCards)
            {
                existingEmoteCard.SetEmoteAsAssignedInSelectedSlot(existingEmoteCard.model.assignedSlot == slotNumber);
            }

            onSlotSelected?.Invoke(emoteId, slotNumber);
        }

        internal List<EmoteCardComponentView> GetAllEmoteCards()
        {
            return emotesGrid
                .GetItems()
                .Select(x => x as EmoteCardComponentView)
                .ToList();
        }

        internal static IEmotesCustomizationComponentView Create()
        {
            EmotesCustomizationComponentView emotesCustomizationComponentView = Instantiate(Resources.Load<GameObject>("EmotesCustomization/EmotesCustomizationSection")).GetComponent<EmotesCustomizationComponentView>();
            emotesCustomizationComponentView.name = "_EmotesCustomizationSection";

            return emotesCustomizationComponentView;
        }
    }
}