using DCL;
using DCL.Helpers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class SceneCardView : MonoBehaviour
{
    public static event Action<ISceneData> OnJumpInPressed;
    public static event Action<ISceneData> OnEditorPressed;
    public static event Action<ISceneData, SceneCardView> OnContextMenuPressed;

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
    [Space]

    [SerializeField] internal GameObject roleOwnerGO;
    [SerializeField] internal GameObject roleOperatorGO;
    [SerializeField] internal GameObject roleContributorGO;

    public SceneSearchInfo searchInfo { get; } = new SceneSearchInfo();

    internal ISceneData sceneData;
    private AssetPromise_Texture thumbnailPromise;

    private void Awake()
    {
        jumpInButton.onClick.AddListener(()=> OnJumpInPressed?.Invoke(sceneData));
        editorButton.onClick.AddListener(()=> OnEditorPressed?.Invoke(sceneData));
        contextMenuButton.onClick.AddListener(()=> OnContextMenuPressed?.Invoke(sceneData, this));
    }

    public void Setup(ISceneData sceneData)
    {
        this.sceneData = sceneData;
        SetThumbnail(sceneData.thumbnailUrl);
        SetName(sceneData.name);
        SetCoords(sceneData.coords);
        SetSize(sceneData.size);
        SetDeployed(sceneData.isDeployed);
        SetUserRole(sceneData.isOwner, sceneData.isOperator, sceneData.isContributor);
        searchInfo.id = sceneData.id;
    }

    public void SetParent(Transform parent)
    {
        if (transform.parent == parent)
            return;

        transform.SetParent(parent);
        transform.ResetLocalTRS();
    }

    public void SetName(string name)
    {
        sceneName.text = name;
        searchInfo.SetName(name);
    }

    public void SetCoords(Vector2Int coords)
    {
        coordsText.text = $"{coords.x},{coords.y}";
    }

    public void SetSize(Vector2Int size)
    {
        sizeText.text = $"{size.x},{size.y}m";
        searchInfo.SetSize(size);
    }

    public void SetThumbnail(string thumbnailUrl)
    {
        if (thumbnailPromise != null)
        {
            AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
            thumbnailPromise = null;
        }

        if (string.IsNullOrEmpty(thumbnailUrl))
        {
            SetThumbnail((Texture2D) null);
            return;
        }

        thumbnailPromise = new AssetPromise_Texture(thumbnailUrl);
        thumbnailPromise.OnSuccessEvent += texture => SetThumbnail(texture.texture);
        thumbnailPromise.OnFailEvent += texture => SetThumbnail((Texture2D) null);

        AssetPromiseKeeper_Texture.i.Keep(thumbnailPromise);
    }

    public void SetThumbnail(Texture2D thumbnailTexture)
    {
        thumbnail.texture = thumbnailTexture ?? defaultThumbnail;
    }

    public void SetDeployed(bool deployed)
    {
        coordsContainer.SetActive(deployed);
        sizeContainer.SetActive(!deployed);
        jumpInButton.gameObject.SetActive(deployed);
    }

    public void SetUserRole(bool isOwner, bool isOperator, bool isContributor)
    {
        roleOwnerGO.SetActive(false);
        roleOperatorGO.SetActive(false);
        roleContributorGO.SetActive(false);
        searchInfo.SetRole(isOwner, isOperator, isContributor);

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

    private void OnDestroy()
    {
        AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
    }
}
