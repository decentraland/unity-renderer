using System;
using DCL;
using TMPro;
using UnityEngine;

public class PreviewMenuPositionView : MonoBehaviour, IDisposable
{
    [SerializeField] internal TMP_InputField xValueInputField;
    [SerializeField] internal TMP_InputField yValueInputField;
    [SerializeField] internal TMP_InputField zValueInputField;

    private bool isDestroyed;

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    internal static string FormatFloatValue(float value)
    {
        return $"{value:0.00}";
    }

    internal void LateUpdate()
    {
        Vector3 position = WorldStateUtils.ConvertUnityToScenePosition(CommonScriptableObjects.playerUnityPosition.Get());
        xValueInputField.text = FormatFloatValue(position.x);
        yValueInputField.text = FormatFloatValue(position.y);
        zValueInputField.text = FormatFloatValue(position.z);
    }
}