using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DCL.Helpers;
using DCL;

public class TeleportPromptHUDView : MonoBehaviour
{
    [SerializeField] internal GameObject content;
    [SerializeField] internal ShowHideAnimator contentAnimator;
    [SerializeField] internal Animator teleportHUDAnimator;

    [Header("Images")]
    [SerializeField] private RawImage imageSceneThumbnail;

    [SerializeField] private Image imageGotoCrowd;
    [SerializeField] private Image imageGotoMagic;

    [Header("Containers")]
    [SerializeField] private GameObject containerCoords;

    [SerializeField] private GameObject containerMagic;
    [SerializeField] private GameObject containerCrowd;
    [SerializeField] private GameObject containerScene;
    [SerializeField] private GameObject containerEvent;

    [Header("Scene info")]
    [SerializeField] private TextMeshProUGUI textCoords;

    [SerializeField] private TextMeshProUGUI textSceneName;
    [SerializeField] private TextMeshProUGUI textSceneOwner;

    [Header("Event info")]
    [SerializeField] private TextMeshProUGUI textEventInfo;

    [SerializeField] private TextMeshProUGUI textEventName;
    [SerializeField] private TextMeshProUGUI textEventAttendees;

    [SerializeField] private Button continueButton;
    [SerializeField] private Button cancelButton;

    public event Action OnCloseEvent;
    public event Action OnTeleportEvent;

    IWebRequestAsyncOperation fetchParcelImageOp;
    Texture2D downloadedBanner;
    private HUDCanvasCameraModeController hudCanvasCameraModeController;
    private static readonly int IDLE = Animator.StringToHash("Idle");
    private static readonly int OUT = Animator.StringToHash("Out");

    private void Awake()
    {
        hudCanvasCameraModeController = new HUDCanvasCameraModeController(content.GetComponent<Canvas>(), DataStore.i.camera.hudsCamera);
        cancelButton.onClick.AddListener(OnClosePressed);
        continueButton.onClick.AddListener(OnTeleportPressed);
        contentAnimator.OnWillFinishHide += (animator) => Hide();
    }

    public void Reset()
    {
        containerCoords.SetActive(false);
        containerCrowd.SetActive(false);
        containerMagic.SetActive(false);
        containerScene.SetActive(false);
        containerEvent.SetActive(false);

        imageSceneThumbnail.gameObject.SetActive(false);
        imageGotoCrowd.gameObject.SetActive(false);
        imageGotoMagic.gameObject.SetActive(false);
    }

    public void SetLoadingCompleted()
    {
        teleportHUDAnimator.SetTrigger(IDLE);
    }

    public void SetOutAnimation()
    {
        teleportHUDAnimator.SetTrigger(OUT);
    }

    public void ShowTeleportToMagic()
    {
        containerMagic.SetActive(true);
        imageGotoMagic.gameObject.SetActive(true);
    }

    public void ShowTeleportToCrowd()
    {
        containerCrowd.SetActive(true);
        imageGotoCrowd.gameObject.SetActive(true);
    }

    public void ShowTeleportToCoords(string coords, string sceneName, string sceneCreator, string previewImageUrl)
    {
        containerCoords.SetActive(true);
        containerScene.SetActive(true);

        textCoords.text = coords;
        textSceneName.text = sceneName;
        textSceneOwner.text = sceneCreator;

        FetchScenePreviewImage(previewImageUrl);
    }

    public void SetEventInfo(string eventName, string eventStatus, int attendeesCount)
    {
        containerEvent.SetActive(true);
        textEventInfo.text = eventStatus;
        textEventName.text = eventName;
        textEventAttendees.text = string.Format("+{0}", attendeesCount);
    }

    private void Hide()
    {
        content.SetActive(false);

        if (fetchParcelImageOp != null)
            fetchParcelImageOp.Dispose();

        if (downloadedBanner != null)
        {
            UnityEngine.Object.Destroy(downloadedBanner);
            downloadedBanner = null;
        }
    }

    private void FetchScenePreviewImage(string previewImageUrl)
    {
        if (string.IsNullOrEmpty(previewImageUrl))
            return;

        fetchParcelImageOp = Utils.FetchTexture(previewImageUrl, false, (texture) =>
        {
            downloadedBanner = texture;
            imageSceneThumbnail.texture = texture;

            RectTransform rt = (RectTransform)imageSceneThumbnail.transform.parent;
            float h = rt.rect.height;
            float w = h * (texture.width / (float)texture.height);
            imageSceneThumbnail.rectTransform.sizeDelta = new Vector2(w, h);
            imageSceneThumbnail.gameObject.SetActive(true);
        });
    }

    private void OnClosePressed()
    {
        OnCloseEvent?.Invoke();
        contentAnimator.Hide(true);

        AudioScriptableObjects.dialogClose.Play(true);
    }

    private void OnTeleportPressed()
    {
        OnTeleportEvent?.Invoke();
        contentAnimator.Hide(true);
    }

    private void OnDestroy()
    {
        hudCanvasCameraModeController?.Dispose();
        if (downloadedBanner != null)
        {
            UnityEngine.Object.Destroy(downloadedBanner);
            downloadedBanner = null;
        }

        if (fetchParcelImageOp != null)
            fetchParcelImageOp.Dispose();
    }
}
