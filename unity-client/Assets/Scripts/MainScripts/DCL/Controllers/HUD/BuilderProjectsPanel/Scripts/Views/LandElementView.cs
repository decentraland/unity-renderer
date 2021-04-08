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
    private const string BUILDER_LAND_URL_FORMAT = "https://builder.decentraland.org/land/{0}";

    public event Action<string> OnJumpInPressed; 
    public event Action<string> OnEditorPressed; 
    public event Action<string> OnSettingsPressed; 
        
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

    public LandSearchInfo searchInfo { get; } = new LandSearchInfo();

    private bool isDestroyed = false;
    private string landId;
    private string landCoordinates;
    private bool isEstate;
    private string thumbnailUrl;
    private AssetPromise_Texture thumbnailPromise;

    private void Awake()
    {
        buttonSettings.onClick.AddListener(()=> OnSettingsPressed?.Invoke(landId));
        buttonJumpIn.onClick.AddListener(()=> OnJumpInPressed?.Invoke(landId));
        buttonEditor.onClick.AddListener(()=> OnEditorPressed?.Invoke(landId));
        
        //NOTE: for MVP we are redirecting user to Builder's page
        OnSettingsPressed += (id) => WebInterface.OpenURL(string.Format(BUILDER_LAND_URL_FORMAT, isEstate ? landId : landCoordinates));
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void SetId(string id)
    {
        landId = id;
        searchInfo.id = id;
    }

    public string GetId()
    {
        return landId;
    }

    public void SetName(string name)
    {
        landName.text = name;
        searchInfo.SetName(name);
    }

    public void SetCoords(int x, int y)
    {
        landCoordinates = $"{x},{y}";
        landCoords.text = landCoordinates;
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

    public void SetIsState(bool isEstate)
    {
        this.isEstate = isEstate;
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
    }

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
        if (thumbnailPromise != null)
        {
            AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
            thumbnailPromise = null;
        }
    }

}
