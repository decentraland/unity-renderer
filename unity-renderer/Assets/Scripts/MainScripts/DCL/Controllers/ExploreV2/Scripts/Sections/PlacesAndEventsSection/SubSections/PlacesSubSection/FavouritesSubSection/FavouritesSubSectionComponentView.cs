using DCL;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FavouritesSubSectionComponentView : BaseComponentView, IFavouritesSubSectionComponentView
{
    internal const string FAVOURITE_CARDS_POOL_NAME = "Places_FavouriteCardsPool";
    private const int FAVOURITE_CARDS_POOL_PREWARM = 20;

    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    [Header("Assets References")]
    [SerializeField] internal PlaceCardComponentView placeCardPrefab;
    [SerializeField] internal PlaceCardComponentView placeCardModalPrefab;

    [Header("Prefab References")]
    [SerializeField] internal ScrollRect scrollView;
    [SerializeField] internal GridContainerComponentView favourites;
    [SerializeField] internal GameObject favouritesLoading;
    [SerializeField] internal GameObject noFavouritesPrompt;
    [SerializeField] internal Color[] friendColors = null;
    [SerializeField] internal GameObject showMoreFavouritesButtonContainer;
    [SerializeField] internal ButtonComponentView showMoreFavouritesButton;

    [SerializeField] private Canvas canvas;

    internal PlaceCardComponentView placeModal;
    internal Pool favouritesCardsPool;
    private Canvas favouritesCanvas;

    public Color[] currentFriendColors => friendColors;

    public int currentPlacesPerRow => favourites.currentItemsPerRow;

    public void SetAllAsLoading() => SetFavouritesAsLoading(true);
    public void SetShowMoreButtonActive(bool isActive) => SetShowMoreFavouritesButtonActive(isActive);
    public int CurrentTilesPerRow => currentPlacesPerRow;

    public event Action OnReady;
    public event Action<PlaceCardComponentModel> OnInfoClicked;
    public event Action<HotScenesController.HotSceneInfo> OnJumpInClicked;
    public event Action<FriendsHandler> OnFriendHandlerAdded;
    public event Action OnFavouriteSubSectionEnable;
    public event Action OnShowMoreFavouritesClicked;

    public override void Awake()
    {
        base.Awake();
        favouritesCanvas = favourites.GetComponent<Canvas>();
    }

    public override void Start()
    {
        placeModal = PlacesAndEventsCardsFactory.GetPlaceCardTemplateHiddenLazy(placeCardModalPrefab);

        favourites.RemoveItems();

        showMoreFavouritesButton.onClick.RemoveAllListeners();
        showMoreFavouritesButton.onClick.AddListener(() => OnShowMoreFavouritesClicked?.Invoke());

        OnReady?.Invoke();
    }

    public override void OnEnable()
    {
        OnFavouriteSubSectionEnable?.Invoke();
    }

    public override void Dispose()
    {
        base.Dispose();
        cancellationTokenSource.Cancel();

        showMoreFavouritesButton.onClick.RemoveAllListeners();

        favourites.Dispose();

        if (placeModal != null)
        {
            placeModal.Dispose();
            Destroy(placeModal.gameObject);
        }
    }

    public void ConfigurePools() =>
        favouritesCardsPool = PlacesAndEventsCardsFactory.GetCardsPoolLazy(FAVOURITE_CARDS_POOL_NAME, placeCardPrefab, FAVOURITE_CARDS_POOL_PREWARM);

    public override void RefreshControl() =>
        favourites.RefreshControl();

    public void SetFavourites(List<PlaceCardComponentModel> places)
    {
        SetFavouritesAsLoading(false);
        noFavouritesPrompt.gameObject.SetActive(places.Count == 0);

        favouritesCardsPool.ReleaseAll();

        this.favourites.ExtractItems();
        this.favourites.RemoveItems();

        SetFavouritesAsync(places, cancellationTokenSource.Token).Forget();
    }

    public void SetFavouritesAsLoading(bool isVisible)
    {
        favouritesCanvas.enabled = !isVisible;

        favouritesLoading.SetActive(isVisible);

        if (isVisible)
            noFavouritesPrompt.gameObject.SetActive(false);
    }

    public void AddFavourites(List<PlaceCardComponentModel> places) =>
        SetFavouritesAsync(places, cancellationTokenSource.Token).Forget();

    private async UniTask SetFavouritesAsync(List<PlaceCardComponentModel> places, CancellationToken cancellationToken)
    {
        foreach (PlaceCardComponentModel place in places)
        {
            PlaceCardComponentView placeCard = PlacesAndEventsCardsFactory.CreateConfiguredPlaceCard(favouritesCardsPool, place, OnInfoClicked, OnJumpInClicked);
            OnFriendHandlerAdded?.Invoke(placeCard.friendsHandler);

            this.favourites.AddItem(placeCard);

            await UniTask.NextFrame(cancellationToken);
        }

        this.favourites.SetItemSizeForModel();
        await favouritesCardsPool.PrewarmAsync(places.Count, cancellationToken);
    }

    public void SetActive(bool isActive)
    {
        canvas.enabled = isActive;

        if (isActive)
            OnEnable();
        else
            OnDisable();
    }

    public void SetShowMoreFavouritesButtonActive(bool isActive) =>
        showMoreFavouritesButtonContainer.gameObject.SetActive(isActive);

    public void ShowPlaceModal(PlaceCardComponentModel placeInfo)
    {
        placeModal.Show();
        PlacesCardsConfigurator.Configure(placeModal, placeInfo, OnInfoClicked, OnJumpInClicked);
    }

    public void HidePlaceModal()
    {
        if (placeModal != null)
            placeModal.Hide();
    }

    public void RestartScrollViewPosition() =>
        scrollView.verticalNormalizedPosition = 1;
}
