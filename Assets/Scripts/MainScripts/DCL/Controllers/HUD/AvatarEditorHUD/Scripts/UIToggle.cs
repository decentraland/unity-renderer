using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public abstract class UIToggle : MonoBehaviour
{
    protected Toggle toggle;

    public bool isOn
    {
        get { return toggle.isOn; }
        set
        {
            if (value != toggle.isOn)
            {
                toggle.isOn = value;
            }
        }
    }

    protected void Awake()
    {
        toggle = GetComponent<Toggle>();
        OnValueChanged(toggle.isOn);
        toggle.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnValueChanged);
    }

    protected abstract void OnValueChanged(bool isOn);
}