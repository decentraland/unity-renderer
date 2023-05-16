using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public interface IToSPopupView : IDisposable
{
    event Action OnAccept;
    event Action OnCancel;
    event Action OnTermsOfServiceLinkPressed;

    void Show();
}

public class ToSPopupView : MonoBehaviour, IToSPopupView, IPointerClickHandler
{
    public event Action OnAccept;
    public event Action OnCancel;
    public event Action OnTermsOfServiceLinkPressed;

    private bool isDestroyed = false;

    [SerializeField] internal ButtonComponentView agreeButton;
    [SerializeField] internal ButtonComponentView cancelButton;
    [SerializeField] internal TextMeshProUGUI tosText;

    private void Awake()
    {
        agreeButton.onClick.AddListener(OnAcceptPressed);
        cancelButton.onClick.AddListener(OnCancelPressed);
    }

    private void OnAcceptPressed()
    {
        OnAccept?.Invoke();
    }

    private void OnCancelPressed()
    {
        OnCancel?.Invoke();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    public void Dispose()
    {
        if (!isDestroyed)
            Destroy(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(tosText, eventData.position, tosText.canvas.worldCamera);
        if (linkIndex == -1)
            return;

        //There should be only one link, we dont even need to get it
        OnTermsOfServiceLinkPressed?.Invoke();
    }
}
