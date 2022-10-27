using DCL;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlacesSubSectionComponentView : BaseComponentView, IPlacesSubSectionComponentView
{
    internal const string PLACE_CARDS_POOL_NAME = "Places_PlaceCardsPool";
    private const int PLACE_CARDS_POOL_PREWARM = 20;

    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

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

    public event Action OnReady;
    public event Action<PlaceCardComponentModel> OnInfoClicked;
    public event Action<HotScenesController.HotSceneInfo> OnJumpInClicked;
    public event Action<FriendsHandler> OnFriendHandlerAdded;
    public event Action OnPlacesSubSectionEnable;
    public event Action OnShowMorePlacesClicked;
    
    public override void Awake()
    {
        base.Awake();
        placesCanvas = places.GetComponent<Canvas>();
    }
    
    public override void Start()
    {
        placeModal = ExplorePlacesUtils.ConfigurePlaceCardModal(placeCardModalPrefab);

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
        cancellationTokenSource.Cancel();

        showMorePlacesButton.onClick.RemoveAllListeners();

        places.Dispose();

        if (placeModal != null)
        {
            placeModal.Dispose();
            Destroy(placeModal.gameObject);
        }
    }
    
    public void ConfigurePools() =>
        ExplorePlacesUtils.ConfigurePlaceCardsPool(out placeCardsPool, PLACE_CARDS_POOL_NAME, placeCardPrefab, PLACE_CARDS_POOL_PREWARM);

    public override void RefreshControl() =>
        places.RefreshControl();
    
    public void SetPlaces(List<PlaceCardComponentModel> places)
    {
        SetPlacesAsLoading(false);
        placesNoDataText.gameObject.SetActive(places.Count == 0);
        
        placeCardsPool.ReleaseAll();
        
        this.places.ExtractItems();
        this.places.RemoveItems();

        SetPlacesAsync(places, cancellationTokenSource.Token).Forget();
    }
    
    public void SetPlacesAsLoading(bool isVisible)
    {
        placesCanvas.enabled = !isVisible;

        placesLoading.SetActive(isVisible);

        if (isVisible)
            placesNoDataText.gameObject.SetActive(false);
    }

    public void AddPlaces(List<PlaceCardComponentModel> places) => 
        SetPlacesAsync(places, cancellationTokenSource.Token).Forget();

    private async UniTask SetPlacesAsync(List<PlaceCardComponentModel> places, CancellationToken cancellationToken)
    {
        foreach (PlaceCardComponentModel place in places)
        {
            this.places.AddItem(
                ExplorePlacesUtils.InstantiateConfiguredPlaceCard(place, placeCardsPool, OnFriendHandlerAdded, OnInfoClicked, OnJumpInClicked));
            await UniTask.NextFrame(cancellationToken);
        }
        
        this.places.SetItemSizeForModel();
        await placeCardsPool.PrewarmAsync(places.Count, cancellationToken);
    }
    
    public void SetActive(bool isActive)
    {
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
        ExplorePlacesUtils.ConfigurePlaceCard(placeModal, placeInfo, OnInfoClicked, OnJumpInClicked);
    }

    public void HidePlaceModal()
    {
        if (placeModal != null)
            placeModal.Hide();
    }

    public void RestartScrollViewPosition() => 
        scrollView.verticalNormalizedPosition = 1;
}