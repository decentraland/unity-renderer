using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BuilderInWorldLoadingTipModel
{
    public string tipMessage;
    public Sprite tipImage;
}

/// <summary>
/// Represents a tip for the BIW loading.
/// </summary>
public class BuilderInWorldLoadingTip : MonoBehaviour
{
    [SerializeField] internal TMP_Text tipText;
    [SerializeField] internal Image tipImage;

    /// <summary>
    /// Configures the tip with a message and an image.
    /// </summary>
    /// <param name="tipModel">Model with the needed tip info.</param>
    public void Configure(BuilderInWorldLoadingTipModel tipModel)
    {
        tipText.text = tipModel.tipMessage;
        tipImage.sprite = tipModel.tipImage;
    }
}