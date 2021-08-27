using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;
using UnityEngine.UI;

public class ImpostorsTestPanel : MonoBehaviour
{
    public Slider movementInterpolationWait;
    public Slider lodDistance;
    public Slider impostorTintMinDistance;
    public Slider impostorTintMaxDistance;
    public Slider impostorTintNearestBlackness;
    public Slider impostorTintFarestBlackness;
    public Slider impostorAlphaNearestValue;
    public Slider impostorAlphaFarestValue;

    private Text movementInterpolationWaitText;
    private Text lodDistanceText;
    private Text impostorTintMinDistanceText;
    private Text impostorTintMaxDistanceText;
    private Text impostorTintNearestBlacknessText;
    private Text impostorTintFarestBlacknessText;
    private Text impostorAlphaNearestValueText;
    private Text impostorAlphaFarestValueText;

    private void Awake()
    {
        SetupSlider(movementInterpolationWait, DataStore.i.avatarsLOD.impostorSettings.movementInterpolationWait, movementInterpolationWaitText);
        SetupSlider(lodDistance, DataStore.i.avatarsLOD.LODDistance, lodDistanceText);
        SetupSlider(impostorTintMinDistance, DataStore.i.avatarsLOD.impostorSettings.tintMinDistance, impostorTintMinDistanceText);
        SetupSlider(impostorTintMaxDistance, DataStore.i.avatarsLOD.impostorSettings.tintMaxDistance, impostorTintMaxDistanceText);
        SetupSlider(impostorTintNearestBlackness, DataStore.i.avatarsLOD.impostorSettings.tintNearestBlackness, impostorTintNearestBlacknessText);
        SetupSlider(impostorTintFarestBlackness, DataStore.i.avatarsLOD.impostorSettings.tintFarestBlackness, impostorTintFarestBlacknessText);
        SetupSlider(impostorAlphaNearestValue, DataStore.i.avatarsLOD.impostorSettings.alphaNearestValue, impostorAlphaNearestValueText);
        SetupSlider(impostorAlphaFarestValue, DataStore.i.avatarsLOD.impostorSettings.alphaFarestValue, impostorAlphaFarestValueText);
    }

    void SetupSlider(Slider targetSlider, BaseVariable<float> dataStoreTargetVariable, Text UITextReference)
    {
        targetSlider.value = dataStoreTargetVariable.Get();
        UITextReference = targetSlider.handleRect.GetComponentInChildren<Text>();
        UITextReference.text = targetSlider.value.ToString("0.00");
        targetSlider.onValueChanged.AddListener((float newValue) =>
        {
            dataStoreTargetVariable.Set(newValue);
            UITextReference.text = newValue.ToString("0.00");
        });
    }
}