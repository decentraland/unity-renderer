using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ButtonComponentModel
{
    public string text;
    public Sprite icon;
    public Button.ButtonClickedEvent onClickEvent;
}