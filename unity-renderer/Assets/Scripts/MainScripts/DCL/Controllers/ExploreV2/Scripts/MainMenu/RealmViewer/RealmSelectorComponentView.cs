using DCL;
using DCL.Interface;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum RealmsSorting
{
    BY_NAME,
    BY_NUMBER_OF_PLAYERS
}

public enum RealmsSortingDirection
{
    ASC,
    DESC
}

public interface IRealmSelectorComponentView
{
    /// <summary>
    /// Set the current realm label.
    /// </summary>
    /// <param name="realm">Current realm.</param>
    void SetCurrentRealm(string realm);

    /// <summary>
    /// Set the available realms component with a list of realms.
    /// </summary>
    /// <param name="realms">List of realms (model) to be loaded.</param>
    void SetAvailableRealms(List<RealmRowComponentModel> realms);
}

public class RealmSelectorComponentView : BaseComponentView, IRealmSelectorComponentView, IComponentModelConfig<RealmSelectorComponentModel>
{
    internal const string REALMS_POOL_NAME = "RealmSelector_RealmRowsPool";
    internal const int REALMS_POOL_PREWARM = 20;

    [Header("Assets References")]
    [SerializeField] internal RealmRowComponentView realmRowPrefab;

    [Header("Prefab References")]
    [SerializeField] internal TMP_Text currentRealmText;
    [SerializeField] internal ButtonComponentView sortByNameButton;
    [SerializeField] internal Image sortByNameArrowUp;
    [SerializeField] internal Image sortByNameArrowDown;
    [SerializeField] internal ButtonComponentView sortByNumberOfPlayersButton;
    [SerializeField] internal Image sortByNumberOfPlayersArrowUp;
    [SerializeField] internal Image sortByNumberOfPlayersArrowDown;
    [SerializeField] internal GridContainerComponentView availableRealms;
    [SerializeField] internal Button modalBackgroundButton;
    [SerializeField] internal ButtonComponentView closeCardButton;
    [SerializeField] internal InputAction_Trigger closeAction;

    [Header("Configuration")]
    [SerializeField] internal RealmSelectorComponentModel model;
    [SerializeField] internal Color colorForEvenRows;
    [SerializeField] internal Color colorForOddRows;
    [SerializeField] internal Color colorForFocusedRows;
    [SerializeField] internal Color colorForActiveSortingArrow;
    [SerializeField] internal Color colorForUnactiveSortingArrow;
    [SerializeField] internal Color[] friendColors = null;

    internal Pool realmsPool;
    internal RealmsSorting currentSorting = RealmsSorting.BY_NUMBER_OF_PLAYERS;
    internal RealmsSortingDirection currentSortingDirection = RealmsSortingDirection.DESC;
    internal RealmTrackerController friendsTrackerController;

    public override void Awake()
    {
        base.Awake();

        friendsTrackerController = new RealmTrackerController(FriendsController.i, friendColors);

        if (sortByNameButton != null)
            sortByNameButton.onClick.AddListener(() =>
            {
                ApplySorting(
                    RealmsSorting.BY_NAME,
                    currentSorting != RealmsSorting.BY_NAME || currentSortingDirection != RealmsSortingDirection.ASC ? RealmsSortingDirection.ASC : RealmsSortingDirection.DESC);

                EventSystem.current?.SetSelectedGameObject(null);
            });

        if (sortByNumberOfPlayersButton != null)
            sortByNumberOfPlayersButton.onClick.AddListener(() =>
            {
                ApplySorting(
                    RealmsSorting.BY_NUMBER_OF_PLAYERS,
                    currentSorting != RealmsSorting.BY_NUMBER_OF_PLAYERS || currentSortingDirection != RealmsSortingDirection.ASC ? RealmsSortingDirection.ASC : RealmsSortingDirection.DESC);

                EventSystem.current?.SetSelectedGameObject(null);
            });

        if (closeCardButton != null)
            closeCardButton.onClick.AddListener(CloseModal);

        if (closeAction != null)
            closeAction.OnTriggered += OnCloseActionTriggered;

        if (modalBackgroundButton != null)
            modalBackgroundButton.onClick.AddListener(CloseModal);

        ConfigureRealmsPool();
        RefreshSortingArrows();
    }

    public void Configure(RealmSelectorComponentModel newModel)
    {
        model = newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetCurrentRealm(model.currentRealmName);
        availableRealms.RefreshControl();
    }

    public override void Show(bool instant = false)
    {
        base.Show(instant);

        DataStore.i.exploreV2.isSomeModalOpen.Set(true);
    }

    public override void Hide(bool instant = false)
    {
        base.Hide(instant);

        DataStore.i.exploreV2.isSomeModalOpen.Set(false);
    }

    public override void Dispose()
    {
        base.Dispose();

        if (sortByNameButton != null)
            sortByNameButton.onClick.RemoveAllListeners();

        if (sortByNameButton != null)
            sortByNumberOfPlayersButton.onClick.RemoveAllListeners();

        if (closeCardButton != null)
            closeCardButton.onClick.RemoveAllListeners();

        if (closeAction != null)
            closeAction.OnTriggered -= OnCloseActionTriggered;

        if (modalBackgroundButton != null)
            modalBackgroundButton.onClick.RemoveAllListeners();
    }

    public void SetCurrentRealm(string realm)
    {
        model.currentRealmName = realm;

        // Set the current realm in the modal header
        if (currentRealmText != null)
            currentRealmText.text = $"You are in <b>{realm.ToUpper()}</b>";

        // Search the current realm in the available ones and set it as connected
        var instantiatedRealms = availableRealms.GetItems();
        foreach (RealmRowComponentView realmRow in instantiatedRealms)
        {
            realmRow.SetAsConnected(realmRow.model.name == realm);
        }
    }

    public void SetAvailableRealms(List<RealmRowComponentModel> realms)
    {
        friendsTrackerController?.RemoveAllHandlers();

        availableRealms.ExtractItems();
        realmsPool.ReleaseAll();

        List<BaseComponentView> realmsToAdd = new List<BaseComponentView>();
        foreach (RealmRowComponentModel realm in realms)
        {
            RealmRowComponentView newRealmRow = realmsPool.Get().gameObject.GetComponent<RealmRowComponentView>();
            newRealmRow.Configure(realm);
            newRealmRow.SetOnHoverColor(colorForFocusedRows);
            newRealmRow.onWarpInClick.RemoveAllListeners();
            newRealmRow.onWarpInClick.AddListener(() =>
            {
                WebInterface.SendChatMessage(new ChatMessage
                {
                    messageType = ChatMessage.Type.NONE,
                    recipient = string.Empty,
                    body = $"/changerealm {realm.name}"
                });
            });
            realmsToAdd.Add(newRealmRow);

            friendsTrackerController?.AddHandler(newRealmRow.friendsHandler);
        }

        availableRealms.SetItems(realmsToAdd);
        RefreshRowColours();

        ApplySorting(currentSorting, currentSortingDirection);
    }

    internal void ApplySorting(RealmsSorting sortBy, RealmsSortingDirection sortingDirection)
    {
        List<BaseComponentView> realmsToSort = availableRealms.ExtractItems();

        switch (sortBy)
        {
            case RealmsSorting.BY_NAME:
                if (sortingDirection == RealmsSortingDirection.ASC)
                    realmsToSort = realmsToSort
                        .OrderBy(d => ((RealmRowComponentView)d).model.name)
                        .ToList();
                else
                    realmsToSort = realmsToSort
                        .OrderByDescending(d => ((RealmRowComponentView)d).model.name)
                        .ToList();
                break;
            case RealmsSorting.BY_NUMBER_OF_PLAYERS:
                if (sortingDirection == RealmsSortingDirection.ASC)
                    realmsToSort = realmsToSort
                        .OrderBy(d => ((RealmRowComponentView)d).model.players)
                        .ToList();
                else
                    realmsToSort = realmsToSort
                        .OrderByDescending(d => ((RealmRowComponentView)d).model.players)
                        .ToList();
                break;
        }

        availableRealms.SetItems(realmsToSort);

        currentSorting = sortBy;
        currentSortingDirection = sortingDirection;

        RefreshSortingArrows();
        RefreshRowColours();
    }

    internal void RefreshSortingArrows()
    {
        sortByNameArrowUp.color = currentSorting == RealmsSorting.BY_NAME && currentSortingDirection == RealmsSortingDirection.ASC ? colorForActiveSortingArrow : colorForUnactiveSortingArrow;
        sortByNameArrowDown.color = currentSorting == RealmsSorting.BY_NAME && currentSortingDirection == RealmsSortingDirection.DESC ? colorForActiveSortingArrow : colorForUnactiveSortingArrow;
        sortByNumberOfPlayersArrowUp.color = currentSorting == RealmsSorting.BY_NUMBER_OF_PLAYERS && currentSortingDirection == RealmsSortingDirection.ASC ? colorForActiveSortingArrow : colorForUnactiveSortingArrow;
        sortByNumberOfPlayersArrowDown.color = currentSorting == RealmsSorting.BY_NUMBER_OF_PLAYERS && currentSortingDirection == RealmsSortingDirection.DESC ? colorForActiveSortingArrow : colorForUnactiveSortingArrow;
    }

    internal void RefreshRowColours()
    {
        var instantiatedRealms = availableRealms.GetItems();
        bool isAnOddRow = true;
        foreach (RealmRowComponentView realmRow in instantiatedRealms)
        {
            realmRow.SetRowColor(isAnOddRow ? colorForOddRows : colorForEvenRows);
            isAnOddRow = !isAnOddRow;
        }
    }

    internal void CloseModal() { Hide(); }

    internal void OnCloseActionTriggered(DCLAction_Trigger action) { CloseModal(); }

    internal void ConfigureRealmsPool()
    {
        realmsPool = PoolManager.i.GetPool(REALMS_POOL_NAME);
        if (realmsPool == null)
        {
            realmsPool = PoolManager.i.AddPool(
                REALMS_POOL_NAME,
                GameObject.Instantiate(realmRowPrefab).gameObject,
                maxPrewarmCount: REALMS_POOL_PREWARM,
                isPersistent: true);

            realmsPool.ForcePrewarm();
        }
    }
}