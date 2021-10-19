using DCL;
using DCL.Helpers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal interface IPlaceCardView : IDisposable
{
    event Action<Vector2Int> OnJumpInPressed;
    event Action<Vector2Int> OnEditorPressed;
    event Action<IPlaceData> OnSettingsPressed;
    event Action<IPlaceData, IPlaceCardView> OnContextMenuPressed;
    IPlaceData PlaceData { get; }
    ISearchInfo searchInfo { get; }
    Vector3 contextMenuButtonPosition { get; }
    void Setup(IPlaceData placeData);
    void SetParent(Transform parent);
    void SetToDefaultParent();
    void ConfigureDefaultParent(Transform parent);
    void SetName(string name);
    void SetCoords(Vector2Int coords);
    void SetSize(Vector2Int size);
    void SetThumbnail(string thumbnailUrl);
    void SetThumbnail(Texture2D thumbnailTexture);
    void SetDeployed(bool deployed);
    void SetEditable(bool isEditable);
    void SetUserRole(bool isOwner, bool isOperator, bool isContributor);
    void SetActive(bool active);
    void SetSiblingIndex(int index);
}

internal class PlaceCardView : MonoBehaviour, IPlaceCardView
{
    const int THMBL_MARKETPLACE_WIDTH = 196;
    const int THMBL_MARKETPLACE_HEIGHT = 143;
    const int THMBL_MARKETPLACE_SIZEFACTOR = 50;

    static readonly Vector3 CONTEXT_MENU_OFFSET = new Vector3(6.24f, 12f, 0);

    public event Action<Vector2Int> OnJumpInPressed;
    public event Action<Vector2Int> OnEditorPressed;
    public event Action<IPlaceData> OnSettingsPressed;
    public event Action<IPlaceData, IPlaceCardView> OnContextMenuPressed;

    [SerializeField] private Texture2D defaultThumbnail;

    [Space]
    [SerializeField] private RawImageFillParent thumbnail;

    [SerializeField] private TextMeshProUGUI sceneName;

    [Space]
    [SerializeField] internal GameObject coordsContainer;

    [SerializeField] private TextMeshProUGUI coordsText;

    [Space]
    [SerializeField] internal GameObject sizeContainer;

    [SerializeField] private TextMeshProUGUI sizeText;

    [Space]
    [SerializeField] internal Button jumpInButton;

    [SerializeField] internal Button editorButton;
    [SerializeField] internal Button contextMenuButton;
    [SerializeField] internal Button settingsButton;

    [Space]
    [SerializeField] internal GameObject roleOwnerGO;

    [SerializeField] internal GameObject roleOperatorGO;
    [SerializeField] internal GameObject roleContributorGO;

    [Space]
    [SerializeField] internal GameObject editorLockedGO;

    [SerializeField] internal GameObject editorLockedTooltipGO;

    [SerializeField] internal Animator loadingAnimator;

    private static readonly int isLoadingAnimation = Animator.StringToHash("isLoading");

    ISearchInfo IPlaceCardView.searchInfo { get; } = new SearchInfo();
    IPlaceData IPlaceCardView.PlaceData => placeData;
    Vector3 IPlaceCardView.contextMenuButtonPosition => contextMenuButton.transform.position + CONTEXT_MENU_OFFSET;

    private IPlaceData placeData;
    private string thumbnailUrl = null;
    private AssetPromise_Texture thumbnailPromise;
    private bool isDestroyed = false;
    private Transform defaultParent;
    private bool isLoadingThumbnail = false;

    private void Awake()
    {
        jumpInButton.onClick.AddListener( JumpInButtonPressed );
        editorButton.onClick.AddListener( EditorButtonPressed );
        contextMenuButton.onClick.AddListener(() => OnContextMenuPressed?.Invoke(placeData, this));
        settingsButton.onClick.AddListener(() => OnSettingsPressed?.Invoke(placeData));

        editorLockedGO.SetActive(false);
        editorLockedTooltipGO.SetActive(false);
    }

    private void JumpInButtonPressed()
    {
        OnJumpInPressed?.Invoke(placeData.coords);
        BIWAnalytics.PlayerJumpOrEdit("Scene", "JumpIn", placeData.coords, "Scene Owner");
    }

    private void EditorButtonPressed()
    {
        OnEditorPressed?.Invoke(placeData.coords);
        BIWAnalytics.PlayerJumpOrEdit("Scene", "Editor", placeData.coords, "Scene Owner");
    }

    private void OnEnable() { loadingAnimator.SetBool(isLoadingAnimation, isLoadingThumbnail); }

    void IPlaceCardView.Setup(IPlaceData placeData)
    {
        this.placeData = placeData;

        string sceneThumbnailUrl = placeData.thumbnailUrl;
        if (string.IsNullOrEmpty(sceneThumbnailUrl) && placeData.parcels != null)
        {
            sceneThumbnailUrl = MapUtils.GetMarketPlaceThumbnailUrl(placeData.parcels,
                THMBL_MARKETPLACE_WIDTH, THMBL_MARKETPLACE_HEIGHT, THMBL_MARKETPLACE_SIZEFACTOR);
        }

        IPlaceCardView thisView = this;
        thisView.SetThumbnail(sceneThumbnailUrl);
        thisView.SetName(placeData.name);
        thisView.SetCoords(placeData.coords);
        thisView.SetSize(placeData.size);
        thisView.SetDeployed(placeData.isDeployed);
        thisView.SetUserRole(placeData.isOwner, placeData.isOperator, placeData.isContributor);
        thisView.SetEditable(placeData.isEditable);

        thisView.searchInfo.SetId(placeData.id);
    }

    void IPlaceCardView.SetParent(Transform parent)
    {
        if (transform.parent == parent)
            return;

        transform.SetParent(parent);
        transform.ResetLocalTRS();
    }

    void IPlaceCardView.SetName(string name)
    {
        sceneName.text = name;
        ((IPlaceCardView)this).searchInfo.SetName(name);
    }

    void IPlaceCardView.SetCoords(Vector2Int coords)
    {
        string coordStr = $"{coords.x},{coords.y}";
        coordsText.text = coordStr;
        ((IPlaceCardView)this).searchInfo.SetCoords(coordStr);
    }

    void IPlaceCardView.SetSize(Vector2Int size)
    {
        sizeText.text = $"{size.x},{size.y}m";
        ((IPlaceCardView)this).searchInfo.SetSize(size.x * size.y);
    }

    void IPlaceCardView.SetThumbnail(string thumbnailUrl)
    {
        if (this.thumbnailUrl == thumbnailUrl)
            return;

        this.thumbnailUrl = thumbnailUrl;

        isLoadingThumbnail = true;
        loadingAnimator.SetBool(isLoadingAnimation, isLoadingThumbnail);

        if (thumbnailPromise != null)
        {
            AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
            thumbnailPromise = null;
        }


        if (string.IsNullOrEmpty(thumbnailUrl))
        {
            ((IPlaceCardView)this).SetThumbnail((Texture2D) null);
            return;
        }

        thumbnailPromise = new AssetPromise_Texture(thumbnailUrl);
        thumbnailPromise.OnSuccessEvent += texture => ((IPlaceCardView)this).SetThumbnail(texture.texture);
        thumbnailPromise.OnFailEvent += texture => ((IPlaceCardView)this).SetThumbnail((Texture2D) null);

        AssetPromiseKeeper_Texture.i.Keep(thumbnailPromise);
    }

    void IPlaceCardView.SetThumbnail(Texture2D thumbnailTexture)
    {
        thumbnail.texture = thumbnailTexture ?? defaultThumbnail;
        isLoadingThumbnail = false;
        loadingAnimator.SetBool(isLoadingAnimation, isLoadingThumbnail);
    }

    void IPlaceCardView.SetDeployed(bool deployed)
    {
        coordsContainer.SetActive(deployed);
        sizeContainer.SetActive(!deployed);
        jumpInButton.gameObject.SetActive(deployed);
    }

    void IPlaceCardView.SetUserRole(bool isOwner, bool isOperator, bool isContributor)
    {
        roleOwnerGO.SetActive(false);
        roleOperatorGO.SetActive(false);
        roleContributorGO.SetActive(false);
        ((IPlaceCardView)this).searchInfo.SetRole(isOwner);

        if (isOwner)
        {
            roleOwnerGO.SetActive(true);
        }
        else if (isOperator)
        {
            roleOperatorGO.SetActive(true);
        }
        else if (isContributor)
        {
            roleContributorGO.SetActive(true);
        }
    }

    void IPlaceCardView.SetActive(bool active) { gameObject.SetActive(active); }

    void IPlaceCardView.SetSiblingIndex(int index) { transform.SetSiblingIndex(index); }
    void IPlaceCardView.SetToDefaultParent() { transform.SetParent(defaultParent); }

    void IPlaceCardView.ConfigureDefaultParent(Transform parent) { defaultParent = parent; }

    void IPlaceCardView.SetEditable(bool isEditable)
    {
        editorButton.gameObject.SetActive(isEditable);
        editorLockedGO.SetActive(!isEditable);
    }

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
        isDestroyed = true;
    }
}