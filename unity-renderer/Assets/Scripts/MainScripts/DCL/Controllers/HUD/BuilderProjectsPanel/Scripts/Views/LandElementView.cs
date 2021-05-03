using System;
using DCL;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

internal class LandElementView : MonoBehaviour, IDisposable
{
    internal const string SIZE_TEXT_FORMAT = "{0} LAND";

    public event Action<Vector2Int> OnJumpInPressed;
    public event Action<Vector2Int> OnEditorPressed;
    public event Action<string> OnSettingsPressed;
    public event Action<string> OnOpenInDappPressed;

    [SerializeField] private Texture2D defaultThumbnail;
    [SerializeField] private RawImageFillParent thumbnail;
    [SerializeField] internal TextMeshProUGUI landName;
    [SerializeField] private TextMeshProUGUI landCoords;
    [SerializeField] internal TextMeshProUGUI landSize;
    [SerializeField] internal GameObject landSizeGO;
    [SerializeField] internal GameObject roleOwner;
    [SerializeField] internal GameObject roleOperator;
    [SerializeField] internal Button buttonSettings;
    [SerializeField] internal Button buttonJumpIn;
    [SerializeField] internal Button buttonEditor;
    [SerializeField] internal Button buttonOpenInBuilderDapp;
    [SerializeField] internal UIHoverTriggerShowHideAnimator editorLocked;
    [SerializeField] private ShowHideAnimator editorLockedTooltipEstate;
    [SerializeField] private ShowHideAnimator editorLockedTooltipSdkScene;
    [SerializeField] internal Animator loadingAnimator;
    
    private static readonly int isLoadingAnimation = Animator.StringToHash("isLoading");

    public ISearchInfo searchInfo { get; } = new SearchInfo();

    private bool isDestroyed = false;
    private string landId;
    private string landCoordinates;
    private string thumbnailUrl;
    private AssetPromise_Texture thumbnailPromise;
    private Vector2Int coords;
    private bool isLoadingThumbnail = false; 

    private void Awake()
    {
        buttonSettings.onClick.AddListener(() => OnSettingsPressed?.Invoke(landId));
        buttonJumpIn.onClick.AddListener(() => OnJumpInPressed?.Invoke(coords));
        buttonEditor.onClick.AddListener(() => OnEditorPressed?.Invoke(coords));
        buttonOpenInBuilderDapp.onClick.AddListener(() => OnOpenInDappPressed?.Invoke(landId));

        editorLockedTooltipEstate.gameObject.SetActive(false);
        editorLockedTooltipSdkScene.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        isDestroyed = true;

        if (thumbnailPromise != null)
        {
            AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
            thumbnailPromise = null;
        }
    }
    
    private void OnEnable()
    {
        loadingAnimator.SetBool(isLoadingAnimation, isLoadingThumbnail);
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void Setup(LandWithAccess land)
    {
        bool estate = land.type == LandType.ESTATE;

        SetId(land.id);
        SetName(land.name);
        SetCoords(land.@base.x, land.@base.y);
        SetSize(land.size);
        SetRole(land.role == LandRole.OWNER);
        SetEditable(!estate);

        if (estate)
        {
            editorLocked.SetShowHideAnimator(editorLockedTooltipEstate);
        }
        else if (land.scenes != null && land.scenes.Count > 0 && land.scenes[0].source == DeployedScene.Source.SDK)
        {
            editorLocked.SetShowHideAnimator(editorLockedTooltipSdkScene);
            SetEditable(false);
        }
    }

    public void SetId(string id)
    {
        landId = id;
        searchInfo.SetId(id);
    }

    public string GetId() { return landId; }

    public void SetName(string name)
    {
        landName.text = name;
        searchInfo.SetName(name);
    }

    public void SetCoords(int x, int y)
    {
        landCoordinates = $"{x},{y}";
        landCoords.text = landCoordinates;
        searchInfo.SetCoords(landCoordinates);
        coords.Set(x, y);
    }

    public void SetSize(int size)
    {
        landSizeGO.SetActive(size > 1);
        landSize.text = string.Format(SIZE_TEXT_FORMAT, size);
        searchInfo.SetSize(size);
    }

    public void SetRole(bool isOwner)
    {
        roleOwner.SetActive(isOwner);
        roleOperator.SetActive(!isOwner);
        searchInfo.SetRole(isOwner);
    }

    public Transform GetParent()
    {
        return transform.parent;
    }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
    }

    public void SetThumbnail(string url)
    {
        if (url == thumbnailUrl)
            return;

        isLoadingThumbnail = true;
        loadingAnimator.SetBool(isLoadingAnimation, isLoadingThumbnail);
        thumbnailUrl = url;

        var prevPromise = thumbnailPromise;

        if (string.IsNullOrEmpty(url))
        {
            SetThumbnail(defaultThumbnail);
        }
        else
        {
            thumbnailPromise = new AssetPromise_Texture(url);
            thumbnailPromise.OnSuccessEvent += asset => SetThumbnail(asset.texture);
            thumbnailPromise.OnFailEvent += asset => SetThumbnail(defaultThumbnail);
            AssetPromiseKeeper_Texture.i.Keep(thumbnailPromise);
        }

        if (prevPromise != null)
        {
            AssetPromiseKeeper_Texture.i.Forget(prevPromise);
        }
    }

    public void SetThumbnail(Texture thumbnailTexture)
    {
        thumbnail.texture = thumbnailTexture;
        isLoadingThumbnail = false;
        loadingAnimator.SetBool(isLoadingAnimation, isLoadingThumbnail);
    }

    public void SetEditable(bool isEditable)
    {
        buttonEditor.gameObject.SetActive(isEditable);
        editorLocked.gameObject.SetActive(!isEditable);
    }

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
    }
}