using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class PreviewMenuVisibilityToggleView : MonoBehaviour
{
    [SerializeField] private Color colorON;
    [SerializeField] private Color colorOFF;
    [SerializeField] private Sprite imageON;
    [SerializeField] private Sprite imageOFF;

    [SerializeField] private TextMeshProUGUI textReference;
    [SerializeField] private Image imageReference;
    [SerializeField] private Button buttonReference;

    private Func<bool> isEnableFunc;
    private Action<bool> onToggleAction;

    public void SetUp(string text, Func<bool> isEnableFunc, Action<bool> onToggleAction)
    {
        textReference.text = text;

        this.isEnableFunc = isEnableFunc;
        this.onToggleAction = onToggleAction;

        bool isEnable = isEnableFunc?.Invoke() ?? false;
        SetToggle(isEnable);
    }

    private void Awake()
    {
        buttonReference.onClick.AddListener(() =>
        {
            bool isEnable = isEnableFunc?.Invoke() ?? false;
            if (onToggleAction != null)
            {
                onToggleAction.Invoke(!isEnable);
                SetToggle(!isEnable);
            }
        });
    }

    private void OnEnable()
    {
        bool isEnable = isEnableFunc?.Invoke() ?? false;
        SetToggle(isEnable);
    }

    private void SetToggle(bool isOn)
    {
        Color color = isOn ? colorON : colorOFF;
        imageReference.sprite = isOn ? imageON : imageOFF;
        imageReference.color = color;
        textReference.color = color;
    }
}