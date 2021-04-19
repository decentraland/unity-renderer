using System;
using UnityEngine;
using UnityEngine.UI;

public interface IBuildModeConfirmationModalView
{
    event Action OnCancelExit;
    event Action OnConfirmExit;

    void SetActive(bool isActive);
    void SetTitle(string text);
    void SetSubTitle(string text);
    void SetCancelButtonText(string text);
    void SetConfirmButtonText(string text);
    void CancelExit();
    void ConfirmExit();
}

public class BuildModeConfirmationModalView : MonoBehaviour, IBuildModeConfirmationModalView
{
    public event Action OnCancelExit;
    public event Action OnConfirmExit;

    [SerializeField] internal TMPro.TMP_Text title;
    [SerializeField] internal TMPro.TMP_Text subTitle;
    [SerializeField] internal TMPro.TMP_Text cancelButtonText;
    [SerializeField] internal TMPro.TMP_Text confirmButtonText;
    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button cancelButton;
    [SerializeField] internal Button confirmButton;

    private const string VIEW_PATH = "Common/BuildModeConfirmationModalView";

    internal static BuildModeConfirmationModalView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<BuildModeConfirmationModalView>();
        view.gameObject.name = "_ExitFromBiWModalView";

        return view;
    }

    private void Awake()
    {
        closeButton.onClick.AddListener(CancelExit);
        cancelButton.onClick.AddListener(CancelExit);
        confirmButton.onClick.AddListener(ConfirmExit);
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveListener(CancelExit);
        cancelButton.onClick.RemoveListener(CancelExit);
        confirmButton.onClick.RemoveListener(ConfirmExit);
    }

    public void SetActive(bool isActive) { gameObject.SetActive(isActive); }

    public void SetTitle(string text) { title.text = text; }

    public void SetSubTitle(string text) { subTitle.text = text; }

    public void SetCancelButtonText(string text) { cancelButtonText.text = text; }

    public void SetConfirmButtonText(string text) { confirmButtonText.text = text; }

    public void CancelExit() { OnCancelExit?.Invoke(); }

    public void ConfirmExit() { OnConfirmExit?.Invoke(); }
}