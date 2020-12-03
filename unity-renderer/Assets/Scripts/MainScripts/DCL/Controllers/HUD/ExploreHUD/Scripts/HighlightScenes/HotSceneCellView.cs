using System;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using TMPro;

internal class HotSceneCellView : MonoBehaviour
{
    const int THMBL_MARKETPLACE_WIDTH = 196;
    const int THMBL_MARKETPLACE_HEIGHT = 143;
    const int THMBL_MARKETPLACE_SIZEFACTOR = 50;

    [Header("Animators")]
    [SerializeField] Animator viewAnimator;
    [SerializeField] ShowHideAnimator jumpInButtonAnimator;

    [Header("Crowd")]
    [SerializeField] GameObject crowdCountContainer;
    [SerializeField] TextMeshProUGUI crowdCount;

    [Header("Events")]
    [SerializeField] GameObject eventsContainer;

    [Header("Friends")]
    [SerializeField] ExploreFriendsView friendsView;
    [SerializeField] GameObject friendsContainer;

    [Header("Scene")]
    [SerializeField] TextMeshProUGUI sceneName;
    [SerializeField] internal RawImageFillParent thumbnailImage;
    [SerializeField] UIHoverCallback sceneInfoButton;

    [Header("UI")]
    [SerializeField] UIHoverCallback jumpInHoverArea;
    [SerializeField] Button_OnPointerDown jumpIn;
    [SerializeField] Sprite errorThumbnail;

    public delegate void JumpInDelegate(Vector2Int coords, string serverName, string layerName);
    public static event JumpInDelegate OnJumpIn;

    public static event Action<HotSceneCellView> OnInfoButtonPointerDown;
    public static event Action OnInfoButtonPointerExit;

    public event Action<Texture2D> OnThumbnailSet;

    public MapInfoHandler mapInfoHandler { private set; get; }
    public FriendsHandler friendsHandler { private set; get; }
    public ThumbnailHandler thumbnailHandler { private set; get; }
    public AnimationHandler animationHandler { private set; get; }

    private ViewPool<ExploreFriendsView> friendPool;
    private Dictionary<string, ExploreFriendsView> friendViewById;
    private bool isLoaded = false;
    private bool isInitialized = false;

    HotScenesController.HotSceneInfo hotSceneInfo;

    protected void Awake()
    {
        crowdCountContainer.SetActive(false);
        eventsContainer.SetActive(false);

        jumpInHoverArea.OnPointerEnter += () =>
        {
            jumpInButtonAnimator.gameObject.SetActive(true);
            jumpInButtonAnimator.Show();
        };
        jumpInHoverArea.OnPointerExit += () => jumpInButtonAnimator.Hide();
        sceneInfoButton.OnPointerDown += () => jumpInButtonAnimator.Hide(true);

        // NOTE: we don't use the pointer down callback to avoid being mistakenly pressed while dragging
        jumpIn.onClick.AddListener(JumpInPressed);

        sceneInfoButton.OnPointerDown += () => OnInfoButtonPointerDown?.Invoke(this);
        sceneInfoButton.OnPointerExit += () => OnInfoButtonPointerExit?.Invoke();

        Initialize();
    }

    public void Initialize()
    {
        if (isInitialized)
            return;

        isInitialized = true;

        friendPool = new ViewPool<ExploreFriendsView>(friendsView, 0);
        friendViewById = new Dictionary<string, ExploreFriendsView>();

        mapInfoHandler = new MapInfoHandler();

        friendsHandler = new FriendsHandler(mapInfoHandler);
        friendsHandler.onFriendAdded += OnFriendAdded;
        friendsHandler.onFriendRemoved += OnFriendRemoved;

        thumbnailHandler = new ThumbnailHandler();
        animationHandler = new AnimationHandler(viewAnimator);
    }

    public void Setup(HotScenesController.HotSceneInfo sceneInfo)
    {
        hotSceneInfo = sceneInfo;
        mapInfoHandler.SetMinimapSceneInfo(sceneInfo);

        OnCrowdInfoUpdated(sceneInfo);
        OnMapInfoUpdated(sceneInfo);
    }

    public void Clear()
    {
        thumbnailImage.texture = null;
        thumbnailHandler.Dispose();
        thumbnailImage.gameObject.SetActive(false);
        isLoaded = false;
    }

    public void JumpInPressed()
    {
        HotScenesController.HotSceneInfo.Realm realm = new HotScenesController.HotSceneInfo.Realm() { layer = null, serverName = null };
        for (int i = 0; i < hotSceneInfo.realms.Length; i++)
        {
            if (hotSceneInfo.realms[i].usersCount < hotSceneInfo.realms[i].usersMax)
            {
                realm = hotSceneInfo.realms[i];
                break;
            }
        }

        OnJumpIn?.Invoke(hotSceneInfo.baseCoords, realm.serverName, realm.layer);
    }

    private void OnDestroy()
    {
        friendPool.Dispose();
        thumbnailHandler.Dispose();

        friendsHandler.onFriendAdded -= OnFriendAdded;
        friendsHandler.onFriendRemoved -= OnFriendRemoved;
    }

    private void OnEnable()
    {
        jumpInButtonAnimator.gameObject.SetActive(false);
        jumpInHoverArea.enabled = isLoaded;
        thumbnailImage.gameObject.SetActive(isLoaded);

        if (isLoaded)
        {
            animationHandler.SetLoaded();
        }
    }

    private void OnCrowdInfoUpdated(HotScenesController.HotSceneInfo info)
    {
        crowdCount.text = info.usersTotalCount.ToString();
        crowdCountContainer.SetActive(info.usersTotalCount > 0);
    }

    private void OnFriendAdded(UserProfile profile, Color backgroundColor)
    {
        var view = friendPool.GetView();
        view.SetUserProfile(profile, backgroundColor);
        friendViewById.Add(profile.userId, view);
    }

    private void OnFriendRemoved(UserProfile profile)
    {
        if (friendViewById.TryGetValue(profile.userId, out ExploreFriendsView view))
        {
            friendPool.PoolView(view);
            friendViewById.Remove(profile.userId);
        }
    }

    private void OnMapInfoUpdated(HotScenesController.HotSceneInfo sceneInfo)
    {
        sceneName.text = sceneInfo.name;

        FetchThumbnail(sceneInfo.thumbnail,
            onFail: () => FetchThumbnail(MapUtils.GetMarketPlaceThumbnailUrl(sceneInfo.parcels, THMBL_MARKETPLACE_WIDTH, THMBL_MARKETPLACE_HEIGHT, THMBL_MARKETPLACE_SIZEFACTOR),
            onFail: () => SetThumbnail(errorThumbnail.texture)));
    }

    private void FetchThumbnail(string url, Action onFail)
    {
        thumbnailHandler.FetchThumbnail(url, SetThumbnail, onFail);
    }

    private void SetThumbnail(Texture2D texture)
    {
        thumbnailImage.texture = texture;
        thumbnailImage.gameObject.SetActive(true);
        OnThumbnailSet?.Invoke(texture);

        SetLoaded();
    }

    private void SetLoaded()
    {
        isLoaded = true;
        animationHandler.SetLoaded();
        jumpInHoverArea.enabled = true;
    }
}
