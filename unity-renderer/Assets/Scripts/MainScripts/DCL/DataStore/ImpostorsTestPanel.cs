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
    public Slider impostorTintMaxDistance;
    public Slider impostorTintNearestBlackness;
    public Slider impostorTintFarestBlackness;
    public Slider impostorAlphaNearestValue;
    public Slider impostorAlphaFarestValue;

    private Text movementInterpolationWaitText;
    private Text lodDistanceText;
    private Text impostorTintMaxDistanceText;
    private Text impostorTintNearestBlacknessText;
    private Text impostorTintFarestBlacknessText;
    private Text impostorAlphaNearestValueText;
    private Text impostorAlphaFarestValueText;

    private void Awake()
    {
        DataStore.i.avatarsLOD.testPanel.impostorsMovementInterpolationWait.Set(movementInterpolationWait.value);
        movementInterpolationWaitText = movementInterpolationWait.handleRect.GetComponentInChildren<Text>();
        movementInterpolationWaitText.text = movementInterpolationWait.value.ToString("0.00");
        movementInterpolationWait.onValueChanged.AddListener((float newValue) =>
        {
            DataStore.i.avatarsLOD.testPanel.impostorsMovementInterpolationWait.Set(newValue);
            movementInterpolationWaitText.text = newValue.ToString("0.00");
        });

        DataStore.i.avatarsLOD.testPanel.lodDistance.Set(lodDistance.value);
        lodDistanceText = lodDistance.handleRect.GetComponentInChildren<Text>();
        lodDistanceText.text = lodDistance.value.ToString("0.00");
        lodDistance.onValueChanged.AddListener((float newValue) =>
        {
            DataStore.i.avatarsLOD.testPanel.lodDistance.Set(newValue);
            lodDistanceText.text = newValue.ToString("0.00");
        });

        DataStore.i.avatarsLOD.testPanel.impostorTintMaxDistance.Set(impostorTintMaxDistance.value);
        impostorTintMaxDistanceText = impostorTintMaxDistance.handleRect.GetComponentInChildren<Text>();
        impostorTintMaxDistanceText.text = impostorTintMaxDistance.value.ToString("0.00");
        impostorTintMaxDistance.onValueChanged.AddListener((float newValue) =>
        {
            DataStore.i.avatarsLOD.testPanel.impostorTintMaxDistance.Set(newValue);
            impostorTintMaxDistanceText.text = newValue.ToString("0.00");
        });

        DataStore.i.avatarsLOD.testPanel.impostorTintNearestBlackness.Set(impostorTintNearestBlackness.value);
        impostorTintNearestBlacknessText = impostorTintNearestBlackness.handleRect.GetComponentInChildren<Text>();
        impostorTintNearestBlacknessText.text = impostorTintNearestBlackness.value.ToString("0.00");
        impostorTintNearestBlackness.onValueChanged.AddListener((float newValue) =>
        {
            DataStore.i.avatarsLOD.testPanel.impostorTintNearestBlackness.Set(newValue);
            impostorTintNearestBlacknessText.text = newValue.ToString("0.00");
        });

        DataStore.i.avatarsLOD.testPanel.impostorTintFarestBlackness.Set(impostorTintFarestBlackness.value);
        impostorTintFarestBlacknessText = impostorTintFarestBlackness.handleRect.GetComponentInChildren<Text>();
        impostorTintFarestBlacknessText.text = impostorTintFarestBlackness.value.ToString("0.00");
        impostorTintFarestBlackness.onValueChanged.AddListener((float newValue) =>
        {
            DataStore.i.avatarsLOD.testPanel.impostorTintFarestBlackness.Set(newValue);
            impostorTintFarestBlacknessText.text = newValue.ToString("0.00");
        });

        DataStore.i.avatarsLOD.testPanel.impostorAlphaNearestValue.Set(impostorAlphaNearestValue.value);
        impostorAlphaNearestValueText = impostorAlphaNearestValue.handleRect.GetComponentInChildren<Text>();
        impostorAlphaNearestValueText.text = impostorAlphaNearestValue.value.ToString("0.00");
        impostorAlphaNearestValue.onValueChanged.AddListener((float newValue) =>
        {
            DataStore.i.avatarsLOD.testPanel.impostorAlphaNearestValue.Set(newValue);
            impostorAlphaNearestValueText.text = newValue.ToString("0.00");
        });

        DataStore.i.avatarsLOD.testPanel.impostorAlphaFarestValue.Set(impostorAlphaFarestValue.value);
        impostorAlphaFarestValueText = impostorAlphaFarestValue.handleRect.GetComponentInChildren<Text>();
        impostorAlphaFarestValueText.text = impostorAlphaFarestValue.value.ToString("0.00");
        impostorAlphaFarestValue.onValueChanged.AddListener((float newValue) =>
        {
            DataStore.i.avatarsLOD.testPanel.impostorAlphaFarestValue.Set(newValue);
            impostorAlphaFarestValueText.text = newValue.ToString("0.00");
        });
    }
}