using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.EmotesCustomization
{
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
        public event Action<string> onSellEmoteClicked;
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
            emoteInfoPanel.closeButton.onClick.AddListener(() => SetEmoteInfoPanelActive(false));

            ConfigureEmotesPool();
        }

        public void Start()
        {
            emoteSlotSelector.SelectSlot(DEFAULT_SELECTED_SLOT);
        }

        public override void RefreshControl()
        {
            emoteSlotSelector.RefreshControl();
            emotesGrid.RefreshControl();
        }

        public override void Dispose()
        {
            CleanEmotes();

            emoteSlotSelector.onSlotSelected -= OnSlotSelected;
            emoteInfoPanel.closeButton.onClick.RemoveAllListeners();
            emoteInfoPanel.sellButton.onClick.RemoveAllListeners();

            base.Dispose();
        }

        public void CleanEmotes()
        {
            emotesGrid.ExtractItems();
            emoteCardsPool.ReleaseAll();
        }

        public EmoteCardComponentView AddEmote(EmoteCardComponentModel emote)
        {
            EmoteCardComponentView emoteGO = InstantiateAndConfigureEmoteCard(emote);
            emotesGrid.AddItemWithResize(emoteGO);

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
                    existingEmoteCard.UnassignSlot();

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

            SetEmoteInfoPanelActive(false);

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

            SetEmoteInfoPanelActive(false);

            if (notifyEvent)
                onEmoteUnequipped?.Invoke(emoteId, slotNumber);
        }

        public void OpenEmoteInfoPanel(EmoteCardComponentModel emoteModel, Color backgroundColor, Transform anchorTransform)
        {
            emoteInfoPanel.SetModel(NFTItemInfo.Model.FromEmoteItem(emoteModel));
            emoteInfoPanel.SetBackgroundColor(backgroundColor);
            emoteInfoPanel.SetRarityName(emoteModel.rarity);
            SetEmoteInfoPanelActive(true);
            emoteInfoPanel.transform.SetParent(anchorTransform);
            emoteInfoPanel.transform.localPosition = Vector3.zero;
            emoteInfoPanel.sellButton.onClick.RemoveAllListeners();
            emoteInfoPanel.sellButton.onClick.AddListener(() => onSellEmoteClicked?.Invoke(emoteModel.id));
        }

        public void SetEmoteInfoPanelActive(bool isActive) { emoteInfoPanel.SetActive(isActive); }

        public EmoteCardComponentView GetEmoteCardById(string emoteId) { return GetAllEmoteCards().FirstOrDefault(x => x.model.id == emoteId); }

        public void SetActive(bool isActive) { gameObject.SetActive(isActive); }

        public EmoteSlotCardComponentView GetSlot(int slotNumber) { return currentSlots.FirstOrDefault(x => x.model.slotNumber == slotNumber); }

        internal void ClickOnEmote(string emoteId, string emoteName, int slotNumber, bool isAssignedInSelectedSlot)
        {
            if (!isAssignedInSelectedSlot)
                EquipEmote(emoteId, emoteName, slotNumber);
            else
                UnequipEmote(emoteId, selectedSlot);

            SetEmoteInfoPanelActive(false);
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

                emoteCardsPool.ForcePrewarm(forceActive: false);
            }
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

            SetEmoteInfoPanelActive(false);
            onSlotSelected?.Invoke(emoteId, slotNumber);
        }

        internal List<EmoteCardComponentView> GetAllEmoteCards()
        {
            return emotesGrid
                .GetItems()
                .Select(x => x as EmoteCardComponentView)
                .ToList();
        }

        internal static IEmotesCustomizationComponentView Create(string path = "EmotesCustomization/EmotesCustomizationSection")
        {
            EmotesCustomizationComponentView emotesCustomizationComponentView = Instantiate(Resources.Load<GameObject>(path)).GetComponent<EmotesCustomizationComponentView>();
            emotesCustomizationComponentView.name = "_EmotesCustomizationSection";

            return emotesCustomizationComponentView;
        }
    }
}
