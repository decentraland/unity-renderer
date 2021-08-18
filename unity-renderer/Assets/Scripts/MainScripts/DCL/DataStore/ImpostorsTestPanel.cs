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
    public Slider maxNonLODAvatars;
    public Slider impostorTintMinDistance;
    public Slider impostorTintMaxDistance;
    public Slider impostorTintNearestBlackness;
    public Slider impostorTintFarestBlackness;
    public Slider impostorAlphaNearestValue;
    public Slider impostorAlphaFarestValue;

    private Text movementInterpolationWaitText;
    private Text lodDistanceText;
    private Text maxNonLODAvatarsText;
    private Text impostorTintMinDistanceText;
    private Text impostorTintMaxDistanceText;
    private Text impostorTintNearestBlacknessText;
    private Text impostorTintFarestBlacknessText;
    private Text impostorAlphaNearestValueText;
    private Text impostorAlphaFarestValueText;

    private void Awake()
    {
        SetupSlider(movementInterpolationWait, DataStore.i.avatarsLOD.testPanel.impostorsMovementInterpolationWait, movementInterpolationWaitText);
        SetupSlider(lodDistance, DataStore.i.avatarsLOD.testPanel.lodDistance, lodDistanceText);
        SetupSlider(maxNonLODAvatars, DataStore.i.avatarsLOD.testPanel.maxNonLODAvatars, maxNonLODAvatarsText);
        SetupSlider(impostorTintMinDistance, DataStore.i.avatarsLOD.testPanel.impostorTintMinDistance, impostorTintMinDistanceText);
        SetupSlider(impostorTintMaxDistance, DataStore.i.avatarsLOD.testPanel.impostorTintMaxDistance, impostorTintMaxDistanceText);
        SetupSlider(impostorTintNearestBlackness, DataStore.i.avatarsLOD.testPanel.impostorTintNearestBlackness, impostorTintNearestBlacknessText);
        SetupSlider(impostorTintFarestBlackness, DataStore.i.avatarsLOD.testPanel.impostorTintFarestBlackness, impostorTintFarestBlacknessText);
        SetupSlider(impostorAlphaNearestValue, DataStore.i.avatarsLOD.testPanel.impostorAlphaNearestValue, impostorAlphaNearestValueText);
        SetupSlider(impostorAlphaFarestValue, DataStore.i.avatarsLOD.testPanel.impostorAlphaFarestValue, impostorAlphaFarestValueText);
    }

    void SetupSlider(Slider targetSlider, BaseVariable<float> dataStoreTargetVariable, Text UITextReference)
    {
        dataStoreTargetVariable.Set(targetSlider.value);
        UITextReference = targetSlider.handleRect.GetComponentInChildren<Text>();
        UITextReference.text = targetSlider.value.ToString("0.00");
        targetSlider.onValueChanged.AddListener((float newValue) =>
        {
            dataStoreTargetVariable.Set(newValue);
            UITextReference.text = newValue.ToString("0.00");
        });
    }
}