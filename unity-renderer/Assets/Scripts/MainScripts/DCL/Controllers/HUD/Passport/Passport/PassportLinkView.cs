using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PassportLinkView : BaseComponentView
{
    [SerializeField] private Button clickLinkButton;
    [SerializeField] private TMP_Text buttonText;

    public event Action<string> OnClickLink;
    private string link;

    public void Start()
    {
        clickLinkButton.onClick.RemoveAllListeners();
        clickLinkButton.onClick.AddListener(() => OnClickLink?.Invoke(link));
    }

    public void SetLink(string passportLink)
    {
        link = passportLink;
    }

    public void SetLinkTitle(string passportLinkTitle)
    {
        buttonText.text = passportLinkTitle.Length > 17 ? $"{passportLinkTitle.Substring(0, 15)}..." : passportLinkTitle;
    }

    public override void RefreshControl()
    {
    }
}
