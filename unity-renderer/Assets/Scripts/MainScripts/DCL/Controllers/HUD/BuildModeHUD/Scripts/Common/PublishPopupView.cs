using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IPublishPopupView
{
    public float currentProgress { get; }

    void PublishStart();
    void PublishEnd(bool isOk, string message);
    void SetPercentage(float newValue);
}

public class PublishPopupView : MonoBehaviour, IPublishPopupView
{
    internal const string VIEW_PATH = "Common/PublishPopupView";
    internal const string TITLE_INITIAL_MESSAGE = "Publishing Scene...";
    internal const string SUCCESS_TITLE_MESSAGE = "Scene Published!";
    internal const string FAIL_TITLE_MESSAGE = "Whoops!";
    internal const string SUCCESS_MESSAGE = "We successfully publish your scene.";
    internal const string FAIL_MESSAGE = "There has been an unexpected error. Please contact the support service on the Discord channel.";

    public float currentProgress => loadingBar.currentPercentage;

    [SerializeField] internal TMP_Text titleText;
    [SerializeField] internal TMP_Text resultText;
    [SerializeField] internal TMP_Text errorDetailsText;
    [SerializeField] internal LoadingBar loadingBar;
    [SerializeField] internal Button closeButton;

    internal static PublishPopupView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<PublishPopupView>();
        view.gameObject.name = "_PublishPopupView";

        return view;
    }

    private void Awake() { closeButton.onClick.AddListener(CloseModal); }

    private void OnDestroy() { closeButton.onClick.RemoveListener(CloseModal); }

    public void PublishStart()
    {
        gameObject.SetActive(true);
        loadingBar.SetActive(true);
        resultText.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
        errorDetailsText.gameObject.SetActive(false);
        titleText.text = TITLE_INITIAL_MESSAGE;
    }

    public void PublishEnd(bool isOk, string message)
    {
        loadingBar.SetActive(false);
        titleText.text = isOk ? SUCCESS_TITLE_MESSAGE : FAIL_TITLE_MESSAGE;
        resultText.text = isOk ? SUCCESS_MESSAGE : FAIL_MESSAGE;
        errorDetailsText.gameObject.SetActive(!isOk);
        errorDetailsText.text = isOk ? "" : message;
        resultText.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);
    }

    public void SetPercentage(float newValue) { loadingBar.SetPercentage(newValue); }

    private void CloseModal() { gameObject.SetActive(false); }
}