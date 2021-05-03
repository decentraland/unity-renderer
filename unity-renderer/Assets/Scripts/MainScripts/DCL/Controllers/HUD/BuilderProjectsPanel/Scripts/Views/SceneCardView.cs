using DCL;
using DCL.Helpers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal interface ISceneCardView : IDisposable
{
    event Action<Vector2Int> OnJumpInPressed;
    event Action<Vector2Int> OnEditorPressed;
    event Action<ISceneData> OnSettingsPressed;
    event Action<ISceneData, ISceneCardView> OnContextMenuPressed;
    ISceneData sceneData { get; }
    ISearchInfo searchInfo { get; }
    Vector3 contextMenuButtonPosition { get; }
    void Setup(ISceneData sceneData);
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

internal class SceneCardView : MonoBehaviour, ISceneCardView
{
    const int THMBL_MARKETPLACE_WIDTH = 196;
    const int THMBL_MARKETPLACE_HEIGHT = 143;
    const int THMBL_MARKETPLACE_SIZEFACTOR = 50;
    
    public event Action<Vector2Int> OnJumpInPressed;
    public event Action<Vector2Int> OnEditorPressed;
    public event Action<ISceneData> OnSettingsPressed;
    public event Action<ISceneData, ISceneCardView> OnContextMenuPressed;

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

    ISearchInfo ISceneCardView.searchInfo { get; } = new SearchInfo();
    ISceneData ISceneCardView.sceneData => sceneData;
    Vector3 ISceneCardView.contextMenuButtonPosition => contextMenuButton.transform.position;
    
    private ISceneData sceneData;
    private AssetPromise_Texture thumbnailPromise;
    private bool isDestroyed = false;
    private Transform defaultParent;
    private bool isLoadingThumbnail = false; 

    private void Awake()
    {
        jumpInButton.onClick.AddListener(()=> OnJumpInPressed?.Invoke(sceneData.coords));
        editorButton.onClick.AddListener(()=> OnEditorPressed?.Invoke(sceneData.coords));
        contextMenuButton.onClick.AddListener(()=> OnContextMenuPressed?.Invoke(sceneData, this));
        settingsButton.onClick.AddListener(()=> OnSettingsPressed?.Invoke(sceneData));
        
        editorLockedGO.SetActive(false);
        editorLockedTooltipGO.SetActive(false);
    }

    private void OnEnable()
    {
        loadingAnimator.SetBool(isLoadingAnimation, isLoadingThumbnail);
    }

    void ISceneCardView.Setup(ISceneData sceneData)
    {
        this.sceneData = sceneData;

        string sceneThumbnailUrl = sceneData.thumbnailUrl;
        if (string.IsNullOrEmpty(sceneThumbnailUrl) && sceneData.parcels != null)
        {
            sceneThumbnailUrl = MapUtils.GetMarketPlaceThumbnailUrl(sceneData.parcels,
                THMBL_MARKETPLACE_WIDTH, THMBL_MARKETPLACE_HEIGHT, THMBL_MARKETPLACE_SIZEFACTOR);
        }

        ISceneCardView thisView = this;
        thisView.SetThumbnail(sceneThumbnailUrl);
        thisView.SetName(sceneData.name);
        thisView.SetCoords(sceneData.coords);
        thisView.SetSize(sceneData.size);
        thisView.SetDeployed(sceneData.isDeployed);
        thisView.SetUserRole(sceneData.isOwner, sceneData.isOperator, sceneData.isContributor);
        thisView.SetEditable(sceneData.isEditable);
        
        thisView.searchInfo.SetId(sceneData.id);
    }

    void ISceneCardView.SetParent(Transform parent)
    {
        if (transform.parent == parent)
            return;

        transform.SetParent(parent);
        transform.ResetLocalTRS();
    }

    void ISceneCardView.SetName(string name)
    {
        sceneName.text = name;
        ((ISceneCardView)this).searchInfo.SetName(name);
    }

    void ISceneCardView.SetCoords(Vector2Int coords)
    {
        string coordStr = $"{coords.x},{coords.y}";
        coordsText.text = coordStr;
        ((ISceneCardView)this).searchInfo.SetCoords(coordStr);
    }

    void ISceneCardView.SetSize(Vector2Int size)
    {
        sizeText.text = $"{size.x},{size.y}m";
        ((ISceneCardView)this).searchInfo.SetSize(size.x * size.y);
    }

    void ISceneCardView.SetThumbnail(string thumbnailUrl)
    {
        isLoadingThumbnail = true;
        loadingAnimator.SetBool(isLoadingAnimation, isLoadingThumbnail);
        
        if (thumbnailPromise != null)
        {
            AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
            thumbnailPromise = null;
        }

        if (string.IsNullOrEmpty(thumbnailUrl))
        {
            ((ISceneCardView)this).SetThumbnail((Texture2D) null);
            return;
        }

        thumbnailPromise = new AssetPromise_Texture(thumbnailUrl);
        thumbnailPromise.OnSuccessEvent += texture => ((ISceneCardView)this).SetThumbnail(texture.texture);
        thumbnailPromise.OnFailEvent += texture => ((ISceneCardView)this).SetThumbnail((Texture2D) null);

        AssetPromiseKeeper_Texture.i.Keep(thumbnailPromise);
    }

    void ISceneCardView.SetThumbnail(Texture2D thumbnailTexture)
    {
        thumbnail.texture = thumbnailTexture ?? defaultThumbnail;
        isLoadingThumbnail = false;
        loadingAnimator.SetBool(isLoadingAnimation, isLoadingThumbnail);
    }

    void ISceneCardView.SetDeployed(bool deployed)
    {
        coordsContainer.SetActive(deployed);
        sizeContainer.SetActive(!deployed);
        jumpInButton.gameObject.SetActive(deployed);
    }

    void ISceneCardView.SetUserRole(bool isOwner, bool isOperator, bool isContributor)
    {
        roleOwnerGO.SetActive(false);
        roleOperatorGO.SetActive(false);
        roleContributorGO.SetActive(false);
        ((ISceneCardView)this).searchInfo.SetRole(isOwner);

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

    void ISceneCardView.SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    void ISceneCardView.SetSiblingIndex(int index)
    {
        transform.SetSiblingIndex(index);
    }
    void ISceneCardView.SetToDefaultParent()
    {
        transform.SetParent(defaultParent);
    }

    void ISceneCardView.ConfigureDefaultParent(Transform parent)
    {
        defaultParent = parent;
    }
    
    void ISceneCardView.SetEditable(bool isEditable)
    {
        editorButton.gameObject.SetActive(isEditable);
        editorLockedGO.SetActive(!isEditable);
        settingsButton.gameObject.SetActive(isEditable);
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
