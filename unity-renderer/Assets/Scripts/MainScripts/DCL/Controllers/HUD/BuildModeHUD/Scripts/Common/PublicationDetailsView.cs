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
    string GetSceneName();
    string GetSceneDescription();
}

public class PublicationDetailsView : MonoBehaviour, IPublicationDetailsView
{
    public event Action OnCancel;
    public event Action<string, string> OnPublish;
    public event Action<string> OnSceneNameChange;

    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button cancelButton;
    [SerializeField] internal Button publishButton;
    [SerializeField] internal TMP_InputField sceneNameInput;
    [SerializeField] internal TMP_Text sceneNameValidationText;
    [SerializeField] internal TMP_InputField sceneDescriptionInput;

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
        closeButton.onClick.AddListener(Cancel);
        cancelButton.onClick.AddListener(Cancel);
        publishButton.onClick.AddListener(Publish);

        sceneNameInput.onValueChanged.AddListener((newText) => OnSceneNameChange?.Invoke(newText));
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveListener(Cancel);
        cancelButton.onClick.RemoveListener(Cancel);
        publishButton.onClick.RemoveListener(Publish);

        sceneNameInput.onValueChanged.RemoveAllListeners();
    }

    public void SetActive(bool isActive) { gameObject.SetActive(isActive); }

    public void Cancel() { OnCancel?.Invoke(); }

    public void Publish() { OnPublish?.Invoke(sceneNameInput.text, sceneDescriptionInput.text); }

    public void SetSceneNameValidationActive(bool isActive) { sceneNameValidationText.enabled = isActive; }

    public void SetSceneName(string newtext) { sceneNameInput.text = newtext; }

    public void SetSceneDescription(string newtext) { sceneDescriptionInput.text = newtext; }

    public void SetPublishButtonActive(bool isActive) { publishButton.interactable = isActive; }

    public string GetSceneName() { return sceneNameInput.text; }

    public string GetSceneDescription() { return sceneDescriptionInput.text; }
}