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

    [Header("Images")]
    [SerializeField] RawImage imageSceneThumbnail;
    [SerializeField] Image imageGotoCrowd;
    [SerializeField] Image imageGotoMagic;

    [Header("Containers")]
    [SerializeField] GameObject containerCoords;
    [SerializeField] GameObject containerMagic;
    [SerializeField] GameObject containerCrowd;
    [SerializeField] GameObject containerScene;
    [SerializeField] GameObject containerEvent;

    [Header("Scene info")]
    [SerializeField] TextMeshProUGUI textCoords;
    [SerializeField] TextMeshProUGUI textSceneName;
    [SerializeField] TextMeshProUGUI textSceneOwner;

    [Header("Event info")]
    [SerializeField] TextMeshProUGUI textEventInfo;
    [SerializeField] TextMeshProUGUI textEventName;
    [SerializeField] TextMeshProUGUI textEventAttendees;

    [Header("Buttons")]
    [SerializeField] Button closeButton;
    [SerializeField] Button continueButton;
    [SerializeField] Button cancelButton;

    [Header("Spinners")]
    [SerializeField] GameObject spinnerGeneral;
    [SerializeField] GameObject spinnerImage;

    public event Action OnCloseEvent;
    public event Action OnTeleportEvent;

    WebRequestAsyncOperation fetchParcelImageOp;
    Texture2D downloadedBanner;

    private void Awake()
    {
        closeButton.onClick.AddListener(OnClosePressed);
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

        spinnerImage.SetActive(false);
        spinnerGeneral.SetActive(false);
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

        spinnerImage.SetActive(true);
        fetchParcelImageOp = Utils.FetchTexture(previewImageUrl, (texture) =>
        {
            downloadedBanner = texture;
            imageSceneThumbnail.texture = texture;

            RectTransform rt = (RectTransform)imageSceneThumbnail.transform.parent;
            float h = rt.rect.height;
            float w = h * (texture.width / (float)texture.height);
            imageSceneThumbnail.rectTransform.sizeDelta = new Vector2(w, h);

            spinnerImage.SetActive(false);
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
        if (downloadedBanner != null)
        {
            UnityEngine.Object.Destroy(downloadedBanner);
            downloadedBanner = null;
        }

        if (fetchParcelImageOp != null)
            fetchParcelImageOp.Dispose();
    }
}