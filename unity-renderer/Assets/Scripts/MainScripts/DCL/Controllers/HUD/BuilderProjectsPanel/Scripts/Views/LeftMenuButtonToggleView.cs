using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class LeftMenuButtonToggleView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public static event Action<LeftMenuButtonToggleView> OnToggleOn;

    [Header("Action")]
    [SerializeField] public SectionId openSection;

    [Header("Settings")]
    [SerializeField] private Color colorBackgroundDefault;
    [SerializeField] private Color colorBackgroundSelected;
    [SerializeField] private Color colorTextDefault;
    [SerializeField] private Color colorTextSelected;

    [Header("References")]
    [SerializeField] private Image imageBackground;
    [SerializeField] private TextMeshProUGUI text;

    public bool isOn
    {
        set
        {
            if (isToggleOn == value)
                return;

            SetIsOnWithoutNotify(value);

            if (value)
            {
                OnToggleOn?.Invoke(this);
            }
        }
        get { return isToggleOn; }
    }

    private bool isToggleOn = false;
    private bool isSetup = false;

    public void Setup()
    {
        if (isSetup)
            return;

        isSetup = true;
        OnToggleOn += OnReceiveToggleOn;
    }

    public void SetIsOnWithoutNotify(bool value)
    {
        isToggleOn = value;

        if (isToggleOn)
        {
            SetSelectColor();
        }
        else
        {
            SetDefaultColor();
        }
    }

    private void OnDestroy()
    {
        OnToggleOn -= OnReceiveToggleOn;
    }

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

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (isOn)
            return;

        isOn = true;
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

    private void OnReceiveToggleOn(LeftMenuButtonToggleView toggle)
    {
        if (!isOn)
            return;

        if (toggle != this)
        {
            isOn = false;
        }
    }
}
