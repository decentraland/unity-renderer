using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ProfileCardComponentModel
{
    public Sprite profilePicture;
    public string profileName;
    public string profileAddress;
    public Button.ButtonClickedEvent onClickEvent;
}