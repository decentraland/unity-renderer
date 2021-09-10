using DCL.Interface;
using UnityEngine;

[RequireComponent(typeof(ButtonComponentView))]
public class JumpInAction : MonoBehaviour
{
    public Vector2Int coords;
    public string serverName;
    public string layerName;

    private ButtonComponentView button;

    private void Awake()
    {
        button = GetComponent<ButtonComponentView>();
        button.SetOnClickAction(JumpIn);
    }

    internal void JumpIn()
    {
        if (string.IsNullOrEmpty(serverName))
            WebInterface.GoTo(coords.x, coords.y);
        else
            WebInterface.JumpIn(coords.x, coords.y, serverName, layerName);
    }
}