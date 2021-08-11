using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IPublicationDetailsView
{
    event Action OnCancel;
    event Action<string, string> OnPublish;
    event Action<string> OnSceneNameChange;

    string currentSceneName { get; }

    void SetActive(bool isActive);
    void Cancel();
    void Publish();
    void SetSceneNameValidationActive(bool isActive);
    void SetSceneName(string newtext);
    void SetSceneDescription(string newtext);
    void SetPublishButtonActive(bool isActive);
    void SetPublicationScreenshot(Texture2D screenshotTexture);
    string GetSceneName();
    string GetSceneDescription();
    Texture2D GetSceneScreenshotTexture();
    void UpdateSceneNameCharCounter();
    void UpdateSceneDescriptionCharCounter();
}

public class PublicationDetailsView : MonoBehaviour, IPublicationDetailsView
{
    public event Action OnCancel;
    public event Action<string, string> OnPublish;
    public event Action<string> OnSceneNameChange;

    [SerializeField] internal Button cancelButton;
    [SerializeField] internal Button publishButton;
    [SerializeField] internal TMP_Text publishButtonText;
    [SerializeField] internal TMP_InputField sceneNameInput;
    [SerializeField] internal TMP_Text sceneNameValidationText;
    [SerializeField] internal TMP_InputField sceneDescriptionInput;
    [SerializeField] internal Image sceneScreenshot;
    [SerializeField] internal TMP_Text sceneNameCharCounterText;
    [SerializeField] internal int sceneNameCharLimit = 30;
    [SerializeField] internal TMP_Text sceneDescriptionCharCounterText;
    [SerializeField] internal int sceneDescriptionCharLimit = 140;

    private const string VIEW_PATH = "Common/PublicationDetailsView";

    public string currentSceneName => sceneNameInput.text;

    internal static PublicationDetailsView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<PublicationDetailsView>();
        view.gameObject.name = "_PublicationDetailsView";

        return view;
    }

    private void Awake()
    {
        cancelButton.onClick.AddListener(Cancel);
        publishButton.onClick.AddListener(Publish);

        sceneNameInput.onValueChanged.AddListener((newText) =>
        {
            UpdateSceneNameCharCounter();
            OnSceneNameChange?.Invoke(newText);
        });

        sceneDescriptionInput.onValueChanged.AddListener((newText) =>
        {
            UpdateSceneDescriptionCharCounter();
        });

        sceneNameInput.characterLimit = sceneNameCharLimit;
        sceneDescriptionInput.characterLimit = sceneDescriptionCharLimit;
    }

    private void OnDestroy()
    {
        cancelButton.onClick.RemoveListener(Cancel);
        publishButton.onClick.RemoveListener(Publish);

        sceneNameInput.onValueChanged.RemoveAllListeners();
        sceneDescriptionInput.onValueChanged.RemoveAllListeners();
    }

    public void SetActive(bool isActive) { gameObject.SetActive(isActive); }

    public void Cancel() { OnCancel?.Invoke(); }

    public void Publish() { OnPublish?.Invoke(sceneNameInput.text, sceneDescriptionInput.text); }

    public void SetSceneNameValidationActive(bool isActive) { sceneNameValidationText.enabled = isActive; }

    public void SetSceneName(string newtext)
    {
        sceneNameInput.text = newtext;
        UpdateSceneNameCharCounter();
    }

    public void SetSceneDescription(string newtext)
    {
        sceneDescriptionInput.text = newtext;
        UpdateSceneDescriptionCharCounter();
    }

    public void SetPublishButtonActive(bool isActive)
    {
        publishButton.interactable = isActive;
        publishButtonText.color = new Color(publishButtonText.color.r, publishButtonText.color.g, publishButtonText.color.b, isActive ? 1f : 0.5f);
    }

    public void SetPublicationScreenshot(Texture2D screenshotTexture) { sceneScreenshot.sprite = Sprite.Create(screenshotTexture, new Rect(0.0f, 0.0f, screenshotTexture.width, screenshotTexture.height), new Vector2(0.5f, 0.5f)); }

    public string GetSceneName() { return sceneNameInput.text; }

    public string GetSceneDescription() { return sceneDescriptionInput.text; }

    public Texture2D GetSceneScreenshotTexture() { return sceneScreenshot.sprite.texture; }

    public void UpdateSceneNameCharCounter() { sceneNameCharCounterText.text = $"{sceneNameInput.text.Length}/{sceneNameCharLimit}"; }

    public void UpdateSceneDescriptionCharCounter() { sceneDescriptionCharCounterText.text = $"{sceneDescriptionInput.text.Length}/{sceneDescriptionCharLimit}"; }
}