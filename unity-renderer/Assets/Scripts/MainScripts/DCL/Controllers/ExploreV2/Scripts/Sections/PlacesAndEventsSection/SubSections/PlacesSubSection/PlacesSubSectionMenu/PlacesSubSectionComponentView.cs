using Cysharp.Threading.Tasks;
using DCL;
using DCL.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MainScripts.DCL.Controllers.HotScenes;

public class PlacesSubSectionComponentView : BaseComponentView, IPlacesSubSectionComponentView
{
    internal const string PLACE_CARDS_POOL_NAME = "Places_PlaceCardsPool";
    private const int PLACE_CARDS_POOL_PREWARM = 20;

    private readonly CancellationTokenSource disposeCts = new ();
    private CancellationTokenSource setPlacesCts = new ();

    [Header("Assets References")]
    [SerializeField] internal PlaceCardComponentView placeCardPrefab;
    [SerializeField] internal PlaceCardComponentView placeCardModalPrefab;

    [Header("Prefab References")]
    [SerializeField] internal ScrollRect scrollView;
    [SerializeField] internal GridContainerComponentView places;
    [SerializeField] internal GameObject placesLoading;
    [SerializeField] internal TMP_Text placesNoDataText;
    [SerializeField] internal Color[] friendColors = null;
    [SerializeField] internal GameObject showMorePlacesButtonContainer;
    [SerializeField] internal ButtonComponentView showMorePlacesButton;

    [SerializeField] private Canvas canvas;

    internal PlaceCardComponentView placeModal;
    internal Pool placeCardsPool;
    private Canvas placesCanvas;

    public Color[] currentFriendColors => friendColors;

    public int currentPlacesPerRow => places.currentItemsPerRow;

    public void SetAllAsLoading() => SetPlacesAsLoading(true);
    public void SetShowMoreButtonActive(bool isActive) => SetShowMorePlacesButtonActive(isActive);
    public int CurrentTilesPerRow => currentPlacesPerRow;
    public int CurrentGoingTilesPerRow { get; }

    public event Action OnReady;
    public event Action<PlaceCardComponentModel> OnInfoClicked;
    public event Action<IHotScenesController.PlaceInfo> OnJumpInClicked;
    public event Action<string, bool> OnFavoriteClicked;
    public event Action<FriendsHandler> OnFriendHandlerAdded;
    public event Action OnPlacesSubSectionEnable;
    public event Action OnShowMorePlacesClicked;

    public override void Awake()
    {
        base.Awake();
        placesCanvas = places.GetComponent<Canvas>();
    }

    public void Start()
    {
        placeModal = PlacesAndEventsCardsFactory.GetPlaceCardTemplateHiddenLazy(placeCardModalPrefab);

        places.RemoveItems();

        showMorePlacesButton.onClick.RemoveAllListeners();
        showMorePlacesButton.onClick.AddListener(() => OnShowMorePlacesClicked?.Invoke());

        OnReady?.Invoke();
    }

    public override void OnEnable()
    {
        OnPlacesSubSectionEnable?.Invoke();
    }

    public override void Dispose()
    {
        base.Dispose();
        disposeCts?.SafeCancelAndDispose();
        setPlacesCts?.SafeCancelAndDispose();

        showMorePlacesButton.onClick.RemoveAllListeners();

        places.Dispose();

        if (placeModal != null)
        {
            placeModal.Dispose();
            Destroy(placeModal.gameObject);
        }
    }

    public void ConfigurePools() =>
        placeCardsPool = PlacesAndEventsCardsFactory.GetCardsPoolLazy(PLACE_CARDS_POOL_NAME, placeCardPrefab, PLACE_CARDS_POOL_PREWARM);

    public override void RefreshControl() =>
        places.RefreshControl();

    public void SetPlaces(List<PlaceCardComponentModel> places)
    {
        SetPlacesAsLoading(false);
        placesNoDataText.gameObject.SetActive(places.Count == 0);

        placeCardsPool.ReleaseAll();

        this.places.ExtractItems();
        this.places.RemoveItems();

        setPlacesCts?.SafeCancelAndDispose();
        setPlacesCts = CancellationTokenSource.CreateLinkedTokenSource(disposeCts.Token);
        AddPlacesAsync(places, setPlacesCts.Token).Forget();
    }

    public void SetPlacesAsLoading(bool isVisible)
    {
        placesCanvas.enabled = !isVisible;

        placesLoading.SetActive(isVisible);

        if (isVisible)
            placesNoDataText.gameObject.SetActive(false);
    }

    public void AddPlaces(List<PlaceCardComponentModel> places) =>
        AddPlacesAsync(places, disposeCts.Token).Forget();

    private async UniTask AddPlacesAsync(List<PlaceCardComponentModel> places, CancellationToken cancellationToken)
    {
        foreach (PlaceCardComponentModel place in places)
        {
            PlaceCardComponentView placeCard = PlacesAndEventsCardsFactory.CreateConfiguredPlaceCard(placeCardsPool, place, OnInfoClicked, OnJumpInClicked, OnFavoriteClicked);
            OnFriendHandlerAdded?.Invoke(placeCard.friendsHandler);

            this.places.AddItem(placeCard);

            await UniTask.NextFrame(cancellationToken);
        }

        this.places.SetItemSizeForModel();
        await placeCardsPool.PrewarmAsync(places.Count, cancellationToken);
    }

    public void SetActive(bool isActive)
    {
        if (canvas.enabled == isActive)
            return;
        canvas.enabled = isActive;

        if (isActive)
            OnEnable();
        else
            OnDisable();
    }

    public void SetShowMorePlacesButtonActive(bool isActive) =>
        showMorePlacesButtonContainer.gameObject.SetActive(isActive);

    public void ShowPlaceModal(PlaceCardComponentModel placeInfo)
    {
        placeModal.Show();
        PlacesCardsConfigurator.Configure(placeModal, placeInfo, OnInfoClicked, OnJumpInClicked, OnFavoriteClicked);
    }

    public void HidePlaceModal()
    {
        if (placeModal != null)
            placeModal.Hide();
    }

    public void RestartScrollViewPosition() =>
        scrollView.verticalNormalizedPosition = 1;
}
