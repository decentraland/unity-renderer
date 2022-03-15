using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface ILandPublisherView
{
    event Action OnCancel;
    event Action OnPublish;
    event Action<string> OnSceneNameChange;

    string currentSceneName { get; }

    void SetActive(bool isActive);
    void Cancel();
    void Publish();
    void SetPublishButtonActive(bool isActive);
    void SetPublicationScreenshot(Texture2D screenshotTexture);
    string GetSceneName();
    string GetSceneDescription();
    Texture2D GetSceneScreenshotTexture();
    void SetSceneName(string name);
    void SetSceneDescription(string desc);
}

public class LandPublisherView : MonoBehaviour, ILandPublisherView
{
    public event Action OnCancel;
    public event Action OnPublish;
    public event Action<string> OnSceneNameChange;

    [SerializeField] internal Button cancelButton;
    [SerializeField] internal Button publishButton;
    [SerializeField] internal TMP_Text publishButtonText;
    [SerializeField] internal LimitInputField nameInputField;
    [SerializeField] internal LimitInputField descriptionInputField;
    [SerializeField] internal Image sceneScreenshot;

    private const string VIEW_PATH = "Land/LandPublisherView";

    public string currentSceneName => nameInputField.GetValue();

    internal static LandPublisherView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<LandPublisherView>();
        view.gameObject.name = "_PublicationDetailsView";

        return view;
    }

    private void Awake()
    {
        cancelButton.onClick.AddListener(Cancel);
        publishButton.onClick.AddListener(Publish);

        nameInputField.OnEmptyValue += DisableNextButton;
        nameInputField.OnLimitReached += DisableNextButton;
        nameInputField.OnInputAvailable += EnableNextButton;
            
        descriptionInputField.OnLimitReached += DisableNextButton;
        descriptionInputField.OnInputAvailable += EnableNextButton;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        cancelButton.onClick.RemoveListener(Cancel);
        publishButton.onClick.RemoveListener(Publish);
        
        nameInputField.OnLimitReached -= DisableNextButton;
        nameInputField.OnInputAvailable -= EnableNextButton;
        nameInputField.OnEmptyValue -= DisableNextButton;
            
        descriptionInputField.OnLimitReached -= DisableNextButton;
        descriptionInputField.OnInputAvailable -= EnableNextButton;
    }
    
    internal void EnableNextButton()
    {
        if(nameInputField.IsInputAvailable() && descriptionInputField.IsInputAvailable())
            publishButton.interactable = true;
    }

    internal void DisableNextButton() { publishButton.interactable = false; }

    public void SetActive(bool isActive) { gameObject.SetActive(isActive); }

    public void Cancel() { OnCancel?.Invoke(); }

    public void Publish() { OnPublish?.Invoke(); }

    public void SetPublishButtonActive(bool isActive)
    {
        publishButton.interactable = isActive;
        publishButtonText.color = new Color(publishButtonText.color.r, publishButtonText.color.g, publishButtonText.color.b, isActive ? 1f : 0.5f);
    }

    public void SetPublicationScreenshot(Texture2D screenshotTexture) { sceneScreenshot.sprite = Sprite.Create(screenshotTexture, new Rect(0.0f, 0.0f, screenshotTexture.width, screenshotTexture.height), new Vector2(0.5f, 0.5f)); }

    public string GetSceneName() { return nameInputField.GetValue(); }

    public string GetSceneDescription() { return descriptionInputField.GetValue(); }

    public Texture2D GetSceneScreenshotTexture() { return sceneScreenshot.sprite.texture; }
    
    public void SetSceneName(string name) { nameInputField.SetText(name); }
    
    public void SetSceneDescription(string desc) { descriptionInputField.SetText(desc); }
}