using DCL;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Emotes
{
    public interface IEmotesCustomizationComponentView
    {
        /// <summary>
        /// It will be triggered when an emote card is clicked.
        /// </summary>
        event Action<string> onEmoteClicked;

        /// <summary>
        /// It will be triggered when an emote is equipped.
        /// </summary>
        event Action<string, int> onEmoteEquipped;

        /// <summary>
        /// It will be triggered when an emote is unequipped.
        /// </summary>
        event Action<string, int> onEmoteUnequipped;

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
        /// Get the current selected card.
        /// </summary>
        EmoteCardComponentView selectedCard { get; }

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
        /// Assign an emote into a specific slot.
        /// </summary>
        /// <param name="emoteId">Emote Id to assign.</param>
        /// <param name="emoteName">Emote name to assign.</param>
        /// <param name="slotNumber">Slot number to assign the emote.</param>
        void EquipEmote(string emoteId, string emoteName, int slotNumber);

        /// <summary>
        /// Unassign an emote from a specific slot.
        /// </summary>
        /// <param name="emoteId">Emote Id to unasign.</param>
        /// <param name="slotNumber">Slot number to unassign the emote.</param>
        void UnequipEmote(string emoteId, int slotNumber);

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
        /// Get an emote slot card by slot number.
        /// </summary>
        /// <param name="slotNumber">Slot number to get.</param>
        /// <returns>An emote slot card.</returns>
        EmoteSlotCardComponentView GetEmoteSlotCardBySlotNumber(int slotNumber);
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

        public event Action<string> onEmoteClicked;
        public event Action<string, int> onEmoteEquipped;
        public event Action<string, int> onEmoteUnequipped;

        internal Pool emoteCardsPool;

        public bool isActive => gameObject.activeInHierarchy;
        public Transform viewTransform => transform;
        public int selectedSlot => emoteSlotSelector.selectedSlot;
        public EmoteCardComponentView selectedCard { get; private set; }

        public override void Awake()
        {
            base.Awake();

            emoteSlotSelector.onSlotSelected += OnSlotSelected;

            ConfigureEmotesPool();
            ConfigureEmoteInfoPanel();
        }

        public override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
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

        public void EquipEmote(string emoteId, string emoteName, int slotNumber)
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
                    emoteSlotSelector.SelectSlot(slotNumber);
                }

                existingEmoteCard.SetEmoteAsAssignedInSelectedSlot(existingEmoteCard.model.assignedSlot == selectedSlot);
            }

            emoteInfoPanel.SetActive(false);

            onEmoteEquipped?.Invoke(emoteId, slotNumber);
        }

        public void UnequipEmote(string emoteId, int slotNumber)
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

        public void CloseEmoteInfoPanel()
        {
            emoteInfoPanel.SetActive(false);
        }

        public EmoteCardComponentView GetEmoteCardById(string emoteId)
        {
            return GetAllEmoteCards().FirstOrDefault(x => x.model.id == emoteId);
        }

        public EmoteSlotCardComponentView GetEmoteSlotCardBySlotNumber(int slotNumber)
        {
            EmoteSlotCardComponentView result = emoteSlotSelector
                .GetAllSlots()
                .FirstOrDefault(x => x.model.slotNumber == slotNumber);

            return result;
        }

        internal void ClickOnEmote(string emoteId)
        {
            onEmoteClicked?.Invoke(emoteId);
            emoteInfoPanel.SetActive(false);
        }

        internal void OnEmoteSelected(string emoteId)
        {
            selectedCard = GetEmoteCardById(emoteId);
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

        internal void ConfigureEmoteInfoPanel()
        {
            emoteInfoPanel.closeButton.onClick.AddListener(() => emoteInfoPanel.SetActive(false));
            emoteInfoPanel.sellButton.onClick.AddListener(() =>
            {

            });
        }

        internal EmoteCardComponentView InstantiateAndConfigureEmoteCard(EmoteCardComponentModel emotesInfo)
        {
            EmoteCardComponentView emoteGO = emoteCardsPool.Get().gameObject.GetComponent<EmoteCardComponentView>();
            emoteGO.Configure(emotesInfo);
            emoteGO.onMainClick.RemoveAllListeners();
            emoteGO.onMainClick.AddListener(() => ClickOnEmote(emoteGO.model.id));
            emoteGO.onEquipClick.RemoveAllListeners();
            emoteGO.onEquipClick.AddListener(() => EquipEmote(emoteGO.model.id, emoteGO.model.name, selectedSlot));
            emoteGO.onUnequipClick.RemoveAllListeners();
            emoteGO.onUnequipClick.AddListener(() => UnequipEmote(emoteGO.model.id, selectedSlot));
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