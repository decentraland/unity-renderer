using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AttributeXYZ : MonoBehaviour
{
    public TMP_InputField xField;
    public TMP_InputField yField;
    public TMP_InputField zField;

    public event Action<Vector3> OnChanged;

    Vector3 currentValue;

    bool isSelected = false;

    private void Awake()
    {
        xField.onValueChanged.AddListener(ChangeXValue);
        xField.onSelect.AddListener(InputSelected);
        xField.onDeselect.AddListener(InputDeselected);


        yField.onValueChanged.AddListener(ChangeYValue);
        yField.onSelect.AddListener(InputSelected);
        yField.onDeselect.AddListener(InputDeselected);

        zField.onValueChanged.AddListener(ChangeZValue);
        zField.onSelect.AddListener(InputSelected);
        zField.onDeselect.AddListener(InputDeselected);
    }

    public void SetValues(Vector3 value)
    {
        if (isSelected)
            return;

        currentValue = value;
        xField.SetTextWithoutNotify(value.x.ToString("0.##"));
        yField.SetTextWithoutNotify(value.y.ToString("0.##"));
        zField.SetTextWithoutNotify(value.z.ToString("0.##"));

    }

    public void ChangeXValue(string value)
    {
        if (!isSelected)
            return;

        currentValue.x = float.Parse(value);
        OnChanged?.Invoke(currentValue);
    }

    public void ChangeYValue(string value)
    {
        if (!isSelected)
            return;

        currentValue.y = float.Parse(value);
        OnChanged?.Invoke(currentValue);
    }

    public void ChangeZValue(string value)
    {
        if (!isSelected)
            return;

        currentValue.z = float.Parse(value);
        OnChanged?.Invoke(currentValue);
    }


    public void InputSelected(string text)
    {
        isSelected = true;
    }

    public void InputDeselected(string text)
    {
        isSelected = false;
    }

}
