using DCL;
using DCL.Interface;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum DimensionsSorting
{
    BY_NAME,
    BY_NUMBER_OF_PLAYERS
}

public enum DimensionsSortingDirection
{
    ASC,
    DESC
}

public interface IDimensionSelectorComponentView
{
    /// <summary>
    /// Set the current dimension label.
    /// </summary>
    /// <param name="dimension">Current dimension.</param>
    void SetCurrentDimension(string dimension);

    /// <summary>
    /// Set the available dimensions component with a list of dimensions.
    /// </summary>
    /// <param name="dimensions">List of dimensions (model) to be loaded.</param>
    void SetAvailableDimensions(List<DimensionRowComponentModel> dimensions);
}

public class DimensionSelectorComponentView : BaseComponentView, IDimensionSelectorComponentView, IComponentModelConfig
{
    internal const string DIMENSIONS_POOL_NAME = "Dimensions_DimesionsRowsPool";
    internal const int DIMENSIONS_POOL_PREWARM = 20;

    [Header("Assets References")]
    [SerializeField] internal DimensionRowComponentView dimensionRowPrefab;

    [Header("Prefab References")]
    [SerializeField] internal TMP_Text currentDimensionText;
    [SerializeField] internal ButtonComponentView sortByNameButton;
    [SerializeField] internal Image sortByNameArrowUp;
    [SerializeField] internal Image sortByNameArrowDown;
    [SerializeField] internal ButtonComponentView sortByNumberOfPlayersButton;
    [SerializeField] internal Image sortByNumberOfPlayersArrowUp;
    [SerializeField] internal Image sortByNumberOfPlayersArrowDown;
    [SerializeField] internal GridContainerComponentView availableDimensions;
    [SerializeField] internal Button modalBackgroundButton;
    [SerializeField] internal ButtonComponentView closeCardButton;
    [SerializeField] internal InputAction_Trigger closeAction;

    [Header("Configuration")]
    [SerializeField] internal DimensionSelectorComponentModel model;
    [SerializeField] internal Color colorForEvenRows;
    [SerializeField] internal Color colorForOddRows;
    [SerializeField] internal Color colorForActiveSortingArrow;
    [SerializeField] internal Color colorForUnactiveSortingArrow;
    [SerializeField] internal Color[] friendColors = null;

    internal Pool dimensionsPool;
    internal DimensionsSorting currentSorting = DimensionsSorting.BY_NUMBER_OF_PLAYERS;
    internal DimensionsSortingDirection currentSortingDirection = DimensionsSortingDirection.DESC;
    internal RealmTrackerController friendsTrackerController;

    public override void Awake()
    {
        base.Awake();

        friendsTrackerController = new RealmTrackerController(FriendsController.i, friendColors);

        if (sortByNameButton != null)
            sortByNameButton.onClick.AddListener(() =>
            {
                ApplySorting(
                    DimensionsSorting.BY_NAME,
                    currentSorting != DimensionsSorting.BY_NAME || currentSortingDirection != DimensionsSortingDirection.ASC ? DimensionsSortingDirection.ASC : DimensionsSortingDirection.DESC);
            });

        if (sortByNameButton != null)
            sortByNumberOfPlayersButton.onClick.AddListener(() =>
            {
                ApplySorting(
                    DimensionsSorting.BY_NUMBER_OF_PLAYERS,
                    currentSorting != DimensionsSorting.BY_NUMBER_OF_PLAYERS || currentSortingDirection != DimensionsSortingDirection.ASC ? DimensionsSortingDirection.ASC : DimensionsSortingDirection.DESC);
            });

        if (closeCardButton != null)
            closeCardButton.onClick.AddListener(CloseModal);

        if (closeAction != null)
            closeAction.OnTriggered += OnCloseActionTriggered;

        if (modalBackgroundButton != null)
            modalBackgroundButton.onClick.AddListener(CloseModal);

        ConfigureDimensionsPool();
        RefreshSortingArrows();
    }

    public void Configure(BaseComponentModel newModel)
    {
        model = (DimensionSelectorComponentModel)newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetCurrentDimension(model.currentDimensionName);
        availableDimensions.RefreshControl();
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

    public void SetCurrentDimension(string dimension)
    {
        model.currentDimensionName = dimension;

        // Set the current dimension in the modal header
        if (currentDimensionText != null)
            currentDimensionText.text = $"You are in <b>{System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dimension.ToLower())}</b>";

        // Search the current dimension in the available ones and set it as connected
        var instantiatedDimensions = availableDimensions.GetItems();
        foreach (DimensionRowComponentView dimensionRow in instantiatedDimensions)
        {
            dimensionRow.SetAsConnected(dimensionRow.model.name == dimension);
        }
    }

    public void SetAvailableDimensions(List<DimensionRowComponentModel> dimensions)
    {
        friendsTrackerController?.RemoveAllHandlers();

        availableDimensions.ExtractItems();
        dimensionsPool.ReleaseAll();

        List<BaseComponentView> dimensionsToAdd = new List<BaseComponentView>();
        bool isAnOddRow = true;
        foreach (DimensionRowComponentModel dimension in dimensions)
        {
            DimensionRowComponentView newDimensionRow = dimensionsPool.Get().gameObject.GetComponent<DimensionRowComponentView>();
            newDimensionRow.Configure(dimension);
            newDimensionRow.SetRowColor(isAnOddRow ? colorForOddRows : colorForEvenRows);
            newDimensionRow.onWarpInClick.RemoveAllListeners();
            newDimensionRow.onWarpInClick.AddListener(() =>
            {
                WebInterface.SendChatMessage(new ChatMessage
                {
                    messageType = ChatMessage.Type.NONE,
                    recipient = string.Empty,
                    body = $"/changerealm {dimension.name}"
                });
            });
            dimensionsToAdd.Add(newDimensionRow);
            isAnOddRow = !isAnOddRow;

            friendsTrackerController?.AddHandler(newDimensionRow.friendsHandler);
        }

        availableDimensions.SetItems(dimensionsToAdd);

        ApplySorting(currentSorting, currentSortingDirection);
    }

    internal void ApplySorting(DimensionsSorting sortBy, DimensionsSortingDirection sortingDirection)
    {
        List<BaseComponentView> dimensionsToSort = availableDimensions.ExtractItems();

        switch (sortBy)
        {
            case DimensionsSorting.BY_NAME:
                if (sortingDirection == DimensionsSortingDirection.ASC)
                    dimensionsToSort = dimensionsToSort
                        .OrderBy(d => ((DimensionRowComponentView)d).model.name)
                        .ToList();
                else
                    dimensionsToSort = dimensionsToSort
                        .OrderByDescending(d => ((DimensionRowComponentView)d).model.name)
                        .ToList();
                break;
            case DimensionsSorting.BY_NUMBER_OF_PLAYERS:
                if (sortingDirection == DimensionsSortingDirection.ASC)
                    dimensionsToSort = dimensionsToSort
                        .OrderBy(d => ((DimensionRowComponentView)d).model.players)
                        .ToList();
                else
                    dimensionsToSort = dimensionsToSort
                        .OrderByDescending(d => ((DimensionRowComponentView)d).model.players)
                        .ToList();
                break;
        }

        availableDimensions.SetItems(dimensionsToSort);

        currentSorting = sortBy;
        currentSortingDirection = sortingDirection;

        RefreshSortingArrows();
    }

    internal void RefreshSortingArrows()
    {
        sortByNameArrowUp.color = currentSorting == DimensionsSorting.BY_NAME && currentSortingDirection == DimensionsSortingDirection.ASC ? colorForActiveSortingArrow : colorForUnactiveSortingArrow;
        sortByNameArrowDown.color = currentSorting == DimensionsSorting.BY_NAME && currentSortingDirection == DimensionsSortingDirection.DESC ? colorForActiveSortingArrow : colorForUnactiveSortingArrow;
        sortByNumberOfPlayersArrowUp.color = currentSorting == DimensionsSorting.BY_NUMBER_OF_PLAYERS && currentSortingDirection == DimensionsSortingDirection.ASC ? colorForActiveSortingArrow : colorForUnactiveSortingArrow;
        sortByNumberOfPlayersArrowDown.color = currentSorting == DimensionsSorting.BY_NUMBER_OF_PLAYERS && currentSortingDirection == DimensionsSortingDirection.DESC ? colorForActiveSortingArrow : colorForUnactiveSortingArrow;
    }

    internal void CloseModal() { Hide(); }

    internal void OnCloseActionTriggered(DCLAction_Trigger action) { CloseModal(); }

    internal void ConfigureDimensionsPool()
    {
        dimensionsPool = PoolManager.i.GetPool(DIMENSIONS_POOL_NAME);
        if (dimensionsPool == null)
        {
            dimensionsPool = PoolManager.i.AddPool(
                DIMENSIONS_POOL_NAME,
                GameObject.Instantiate(dimensionRowPrefab).gameObject,
                maxPrewarmCount: DIMENSIONS_POOL_PREWARM,
                isPersistent: true);

            dimensionsPool.ForcePrewarm();
        }
    }
}