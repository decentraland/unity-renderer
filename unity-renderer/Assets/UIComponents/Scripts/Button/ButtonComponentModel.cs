using System;
using UnityEngine;

[Serializable]
public class ButtonComponentModel
{
    public string text;
    public Sprite icon;
    public Action onClickAction;
}