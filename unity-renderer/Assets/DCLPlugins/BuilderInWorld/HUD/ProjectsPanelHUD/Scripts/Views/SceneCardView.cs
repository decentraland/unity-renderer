using DCL;
using DCL.Helpers;
using System;
using DCL.Builder;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This interface represent a scene card 
/// </summary>
internal interface ISceneCardView : IDisposable
{
    /// <summary>
    /// Jump button pressed
    /// </summary>
    event Action<Vector2Int> OnJumpInPressed;

    /// <summary>
    /// Editor button pressed
    /// </summary>
    event Action<Vector2Int> OnEditorPressed;

    /// <summary>
    /// Setting button presed
    /// </summary>
    event Action<ISceneData> OnSettingsPressed;

    /// <summary>
    /// Context button present
    /// </summary>
    event Action<ISceneData, ISceneCardView> OnContextMenuPressed;

    /// <summary>
    /// Data of the scene
    /// </summary>
    ISceneData SceneData { get; }

    /// <summary>
    /// Info of the search
    /// </summary>
    ISearchInfo searchInfo { get; }

    /// <summary>
    /// Position of the context menu
    /// </summary>
    Vector3 contextMenuButtonPosition { get; }

    /// <summary>
    /// This set the data of the card
    /// </summary>
    /// <param name="sceneData">data of the card</param>
    void Setup(ISceneData sceneData);

    /// <summary>
    /// Set Parent of the card
    /// </summary>
    /// <param name="parent"></param>
    void SetParent(Transform parent);

    /// <summary>
    /// Reset to default parent
    /// </summary>
    void SetToDefaultParent();

    /// <summary>
    /// Configure the default parent
    /// </summary>
    /// <param name="parent">default parent to apply</param>
    void ConfigureDefaultParent(Transform parent);

    /// <summary>
    /// This set the order of the card
    /// </summary>
    /// <param name="index"></param>
    void SetSiblingIndex(int index);

    void SetName(string name);
    void SetCoords(Vector2Int coords);
    void SetSize(Vector2Int size);
    void SetThumbnail(string thumbnailUrl);
    void SetThumbnail(Texture2D thumbnailTexture);
    void SetDeployed(bool deployed);
    void SetEditable(bool isEditable);
    void SetUserRole(bool isOwner, bool isOperator, bool isContributor);
    void SetActive(bool active);
}

internal class SceneCardView : MonoBehaviour, ISceneCardView
{
    const int THMBL_MARKETPLACE_WIDTH = 196;
    const int THMBL_MARKETPLACE_HEIGHT = 143;
    const int THMBL_MARKETPLACE_SIZEFACTOR = 50;

    static readonly Vector3 CONTEXT_MENU_OFFSET = new Vector3(6.24f, 12f, 0);

    public event Action<Vector2Int> OnJumpInPressed;
    public event Action<Vector2Int> OnEditorPressed;
    public event Action<ISceneData> OnSettingsPressed;
    public event Action<ISceneData, ISceneCardView> OnContextMenuPressed;

    [SerializeField] private Texture2D defaultThumbnail;

    [Space]
    [SerializeField] private RawImageFillParent thumbnail;
    [SerializeField] private GameObject thumbnailGameObject;
    [SerializeField] private GameObject thumbnailLoading;

    [SerializeField] private TextMeshProUGUI sceneName;

    [Space]
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

    [Space]
    [SerializeField] internal GameObject editorLockedGO;

    [SerializeField] internal GameObject editorLockedTooltipGO;

    [SerializeField] internal Animator loadingAnimator;

    private static readonly int isLoadingAnimation = Animator.StringToHash("isLoading");

    ISearchInfo ISceneCardView.searchInfo { get; } = new SearchInfo();
    ISceneData ISceneCardView.SceneData => sceneData;
    Vector3 ISceneCardView.contextMenuButtonPosition => contextMenuButton.transform.position + CONTEXT_MENU_OFFSET;

    private ISceneData sceneData;
    private string thumbnailUrl = null;
    private AssetPromise_Texture thumbnailPromise;
    private bool isDestroyed = false;
    private Transform defaultParent;
    private bool isLoadingThumbnail = false;

    private void Awake()
    {
        jumpInButton.onClick.AddListener( JumpInButtonPressed );
        editorButton.onClick.AddListener( EditorButtonPressed );
        contextMenuButton.onClick.AddListener(() => OnContextMenuPressed?.Invoke(sceneData, this));
        settingsButton?.onClick.AddListener(() => OnSettingsPressed?.Invoke(sceneData));

        editorLockedGO.SetActive(false);
        editorLockedTooltipGO.SetActive(false);
    }

    private void JumpInButtonPressed()
    {
        OnJumpInPressed?.Invoke(sceneData.coords);
        BIWAnalytics.PlayerJumpOrEdit("Scene", "JumpIn", sceneData.coords, "Scene Owner");
    }

    private void EditorButtonPressed()
    {
        OnEditorPressed?.Invoke(sceneData.coords);
        BIWAnalytics.PlayerJumpOrEdit("Scene", "Editor", sceneData.coords, "Scene Owner");
    }

    private void OnEnable()
    {
        if(loadingAnimator != null)
            loadingAnimator?.SetBool(isLoadingAnimation, isLoadingThumbnail);
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
        if(sizeText == null)
            return;
        sizeText.text = $"{size.x},{size.y}m";
        ((ISceneCardView)this).searchInfo.SetSize(size.x * size.y);
    }

    void ISceneCardView.SetThumbnail(string thumbnailUrl)
    {
        if (this.thumbnailUrl == thumbnailUrl)
            return;

        this.thumbnailUrl = thumbnailUrl;

        isLoadingThumbnail = true;
        thumbnailLoading.SetActive(true);
        if(loadingAnimator != null)
            loadingAnimator?.SetBool(isLoadingAnimation, isLoadingThumbnail);

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
        thumbnailPromise.OnFailEvent += (texture, error) => ((ISceneCardView)this).SetThumbnail((Texture2D) null);

        AssetPromiseKeeper_Texture.i.Keep(thumbnailPromise);
    }

    void ISceneCardView.SetThumbnail(Texture2D thumbnailTexture)
    {
        thumbnail.texture = thumbnailTexture ?? defaultThumbnail;
        isLoadingThumbnail = false;
        thumbnail.enabled = true;
        thumbnailGameObject.SetActive(true);
        thumbnailLoading.SetActive(false);
        if(loadingAnimator != null)
            loadingAnimator?.SetBool(isLoadingAnimation, isLoadingThumbnail);
    }

    void ISceneCardView.SetDeployed(bool deployed)
    {
        if(sizeContainer != null)
            sizeContainer.SetActive(!deployed);
        jumpInButton.gameObject.SetActive(deployed);
    }

    void ISceneCardView.SetUserRole(bool isOwner, bool isOperator, bool isContributor)
    {
        roleOwnerGO.SetActive(false);
        roleOperatorGO.SetActive(false);
        ((ISceneCardView)this).searchInfo.SetRole(isOwner);

        if (isOwner)
        {
            roleOwnerGO.SetActive(true);
        }
        else if (isOperator)
        {
            roleOperatorGO.SetActive(true);
        }
    }

    void ISceneCardView.SetActive(bool active) { gameObject.SetActive(active); }

    void ISceneCardView.SetSiblingIndex(int index) { transform.SetSiblingIndex(index); }
    void ISceneCardView.SetToDefaultParent() { transform.SetParent(defaultParent); }

    void ISceneCardView.ConfigureDefaultParent(Transform parent) { defaultParent = parent; }

    void ISceneCardView.SetEditable(bool isEditable)
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