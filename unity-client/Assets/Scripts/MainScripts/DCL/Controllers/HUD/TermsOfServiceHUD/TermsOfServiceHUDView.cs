using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TermsOfServiceHUDView : MonoBehaviour
{
    private const string VIEW_PATH = "TermsOfServiceHUD";

    private const string SCENE_NAME_VAR = "$sceneName";
    private static readonly string TITLE = $"Terms of Service - {SCENE_NAME_VAR}";
    private static readonly string DESCRIPTION = $"Welcome to {SCENE_NAME_VAR}. Before you proceed, please read carefully.";

    public static TermsOfServiceHUDView CreateView() => Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<TermsOfServiceHUDView>();

    [SerializeField] internal GameObject content;
    [SerializeField] internal TextMeshProUGUI titleText;
    [SerializeField] internal TextMeshProUGUI descriptionText;
    [SerializeField] internal GameObject adultContent;
    [SerializeField] internal GameObject gamblingContent;
    [SerializeField] internal Button agreedButton;
    [SerializeField] internal Button declinedButton;
    [SerializeField] internal Toggle dontShowAgainToggle;


    [Header("Hyperlinks")]
    [SerializeField] internal Button tosLinkButton;
    [SerializeField] internal Button privacyLinkButton;
    [SerializeField] internal Button emailLinkButton;

    public void Initialize(Action<bool> agreedCallback, Action<bool> declinedCallback, Action tosClickedCallback, Action privacyClickedCallback, Action emailClickedCallback)
    {
        agreedButton.onClick.RemoveAllListeners();
        agreedButton.onClick.AddListener(() => agreedCallback?.Invoke(dontShowAgainToggle.isOn));

        declinedButton.onClick.RemoveAllListeners();
        declinedButton.onClick.AddListener(() => declinedCallback?.Invoke(dontShowAgainToggle.isOn));

        tosLinkButton.onClick.RemoveAllListeners();
        tosLinkButton.onClick.AddListener(() => tosClickedCallback?.Invoke());

        privacyLinkButton.onClick.RemoveAllListeners();
        privacyLinkButton.onClick.AddListener(() => privacyClickedCallback?.Invoke());

        emailLinkButton.onClick.RemoveAllListeners();
        emailLinkButton.onClick.AddListener(() => emailClickedCallback?.Invoke());

        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        content.SetActive(visible);

        if (visible)
        {
            AudioScriptableObjects.dialogOpen.Play(true);
        }
        else
        {
            AudioScriptableObjects.dialogClose.Play(true);
        }
    }

    public void SetData(string sceneName, bool hasAdultContent, bool hasGamblingContent, bool hasToS, bool hasPrivacyPolicy, bool hasContactEmail)
    {
        titleText.text = TITLE.Replace(SCENE_NAME_VAR, sceneName);
        descriptionText.text = DESCRIPTION.Replace(SCENE_NAME_VAR, sceneName);

        adultContent.SetActive(hasAdultContent);
        gamblingContent.SetActive(hasGamblingContent);

        tosLinkButton.interactable  = hasToS;
        privacyLinkButton.interactable  = hasPrivacyPolicy;
        emailLinkButton.interactable  = hasContactEmail;

        dontShowAgainToggle.isOn = false;
    }
}