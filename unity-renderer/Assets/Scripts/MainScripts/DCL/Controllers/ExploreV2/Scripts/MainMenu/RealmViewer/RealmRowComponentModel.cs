using System;
using UnityEngine;

[Serializable]
public class RealmRowComponentModel : BaseComponentModel
{
    public string name;
    public int players;
    public bool isConnected;
    public Color backgroundColor;
    public Color onHoverColor;
}