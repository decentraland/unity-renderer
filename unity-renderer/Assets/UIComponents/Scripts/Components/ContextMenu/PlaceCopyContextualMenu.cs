using System;
using UIComponents.ContextMenu;
using UnityEngine;
using UnityEngine.UI;

public class PlaceCopyContextualMenu : ContextMenuComponentView
{
    [SerializeField] internal Button twitterButton;
    [SerializeField] internal Button copyLinkButton;
    [SerializeField] internal Button closeButton;
    [SerializeField] internal ShowHideAnimator nameCopiedToast;

    public event Action OnTwitter;
    public event Action OnPlaceLinkCopied;

    public override void Awake()
    {
        base.Awake();

        twitterButton.onClick.AddListener(() =>
        {
            OnTwitter?.Invoke();
            Hide();
        });

        closeButton.onClick.AddListener(() => Hide());
        copyLinkButton.onClick.AddListener(() =>
        {
            OnPlaceLinkCopied?.Invoke();

            nameCopiedToast.gameObject.SetActive(true);
            nameCopiedToast.ShowDelayHide(3);
        });

        RefreshControl();
    }

    public override void RefreshControl()
    {
    }

    public override void Show(bool instant = false)
    {
        base.Show(instant);
        gameObject.SetActive(true);
        ClampPositionToScreenBorders(transform.position);
    }

    public override void Hide(bool instant = false)
    {
        base.Hide(instant);
        gameObject.SetActive(false);
    }

}
