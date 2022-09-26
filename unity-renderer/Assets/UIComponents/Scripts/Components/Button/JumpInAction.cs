using DCL.Interface;
using System.Diagnostics.CodeAnalysis;
using DCL;
using Unity.Profiling;
using UnityEngine;

[ExcludeFromCodeCoverage]
[RequireComponent(typeof(ButtonComponentView))]
public class JumpInAction : MonoBehaviour
{
    public Vector2Int coords;
    public string serverName;
    public string layerName;

    internal ButtonComponentView button;

    private void Awake()
    {
        button = GetComponent<ButtonComponentView>();

        if (button != null)
            button.onClick.AddListener(JumpIn);
    }

    internal void JumpIn()
    {
        if (string.IsNullOrEmpty(serverName))
            Environment.i.world.teleportController.Teleport(coords.x,coords.y);
        else
            Environment.i.world.teleportController.JumpIn(coords.x,coords.y,serverName, layerName);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(JumpIn);
    }
}