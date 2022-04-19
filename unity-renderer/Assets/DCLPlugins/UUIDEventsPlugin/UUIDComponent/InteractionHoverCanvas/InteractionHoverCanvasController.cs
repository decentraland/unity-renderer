using DCL.Models;
using DCL.Components;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using DCL;

public class InteractionHoverCanvasController : MonoBehaviour
{
    public Canvas canvas;
    public RectTransform backgroundTransform;
    public TextMeshProUGUI text;
    public GameObject[] icons;

    bool isHovered = false;
    GameObject hoverIcon;
    Vector3 meshCenteredPos;

    const string ACTION_BUTTON_POINTER = "POINTER";
    const string ACTION_BUTTON_PRIMARY = "PRIMARY";
    const string ACTION_BUTTON_SECONDARY = "SECONDARY";

    void Awake()
    {
        backgroundTransform.gameObject.SetActive(false);
    }

    public void Setup(string button, string feedbackText)
    {
        text.text = feedbackText;
        ConfigureIcon(button);
        canvas.enabled = enabled && isHovered;
    }

    void ConfigureIcon(string button)
    {
        hoverIcon?.SetActive(false);

        switch (button)
        {
            case ACTION_BUTTON_POINTER:
                hoverIcon = icons[0];
                break;
            case ACTION_BUTTON_PRIMARY:
                hoverIcon = icons[1];
                break;
            case ACTION_BUTTON_SECONDARY:
                hoverIcon = icons[2];
                break;
            default: // ANY
                hoverIcon = icons[3];
                break;
        }

        hoverIcon.SetActive(true);
        backgroundTransform.gameObject.SetActive(true);
    }

    public void SetHoverState(bool hoverState)
    {
        if (!enabled || hoverState == isHovered)
            return;

        isHovered = hoverState;

        canvas.enabled = isHovered;
    }

    public GameObject GetCurrentHoverIcon()
    {
        return hoverIcon;
    }
}