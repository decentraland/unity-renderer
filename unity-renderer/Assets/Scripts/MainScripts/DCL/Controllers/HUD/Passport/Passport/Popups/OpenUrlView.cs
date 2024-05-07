using DCL.Helpers;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenUrlView : BaseComponentView
{
    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button continueButton;
    [SerializeField] internal Button cancelButton;
    [SerializeField] internal TextMeshProUGUI domainText;
    [SerializeField] internal TextMeshProUGUI urlText;
    private string currentUrl;

    public void Start()
    {
        closeButton.onClick.AddListener(() => SetVisibility(false));
        cancelButton.onClick.AddListener(() => SetVisibility(false));
        continueButton.onClick.AddListener(OpenLink);
    }

    public override void RefreshControl()
    {
    }

    private void OpenLink()
    {
        Utils.UnlockCursor();
        WebInterface.OpenURL(currentUrl);
        AnalyticsHelper.SendExternalLinkAnalytic(currentUrl);
        SetVisibility(true);
    }

    public void SetUrlInfo(string url, string domain)
    {
        currentUrl = url;
        urlText.text = url;
        domainText.text = domain;
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            gameObject.SetActive(true);
            Show();
        }
        else
            Hide();
    }
}
