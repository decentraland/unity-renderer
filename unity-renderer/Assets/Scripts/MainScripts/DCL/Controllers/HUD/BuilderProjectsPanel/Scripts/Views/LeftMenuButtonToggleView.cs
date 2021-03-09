using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class LeftMenuButtonToggleView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<bool> OnToggleValueChanged;

    [Header("Settings")]
    [SerializeField] private Color colorBackgroundDefault;
    [SerializeField] private Color colorBackgroundSelected;
    [SerializeField] private Color colorTextDefault;
    [SerializeField] private Color colorTextSelected;

    [Header("References")]
    [SerializeField] private Image imageBackground;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Toggle toggle;

    public bool isOn { set { toggle.isOn = value; } get { return toggle.isOn; } }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (isOn)
            return;

        SetSelectColor();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (isOn)
            return;

        SetDefaultColor();
    }

    private void Awake()
    {
        toggle.onValueChanged.AddListener(value =>
        {
            if (value)
                SetSelectColor();
            else
                SetDefaultColor();

            OnToggleValueChanged?.Invoke(value);
        });
    }

    private void Start()
    {
        if (isOn)
        {
            SetSelectColor();
        }
        else
        {
            SetDefaultColor();
        }
        OnToggleValueChanged?.Invoke(isOn);
    }

    private void SetSelectColor()
    {
        imageBackground.color = colorBackgroundSelected;
        text.color = colorTextSelected;
    }

    private void SetDefaultColor()
    {
        imageBackground.color = colorBackgroundDefault;
        text.color = colorTextDefault;
    }
}
