using DCL;
using TMPro;
using UnityEngine;

public class PreviewMenuPositionView : MonoBehaviour
{
    [SerializeField] private TMP_InputField xValueInputField;
    [SerializeField] private TMP_InputField yValueInputField;
    [SerializeField] private TMP_InputField zValueInputField;

    private string FormatFloatValue(float value)
    {
        return $"{value:0.00}";
    }

    private void LateUpdate()
    {
        Vector3 position = WorldStateUtils.ConvertUnityToScenePosition(CommonScriptableObjects.playerUnityPosition.Get());
        xValueInputField.text = FormatFloatValue(position.x);
        yValueInputField.text = FormatFloatValue(position.y);
        zValueInputField.text = FormatFloatValue(position.z);
    }
}