using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class PreviewMenuVisibilityToggleView : MonoBehaviour, IDisposable
{
    [SerializeField] internal Color colorON;
    [SerializeField] internal Color colorOFF;
    [SerializeField] internal Sprite imageON;
    [SerializeField] internal Sprite imageOFF;

    [SerializeField] internal TextMeshProUGUI textReference;
    [SerializeField] internal Image imageReference;
    [SerializeField] internal Button buttonReference;

    private Func<bool> isEnableFunc;
    private Action<bool> onToggleAction;

    private bool isDestroyed;

    public void SetUp(string text, Func<bool> isEnableFunc, Action<bool> onToggleAction)
    {
        textReference.text = text;

        this.isEnableFunc = isEnableFunc;
        this.onToggleAction = onToggleAction;

        bool isEnable = isEnableFunc?.Invoke() ?? false;
        SetToggle(isEnable);
    }

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
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

    private void OnDestroy()
    {
        isDestroyed = true;
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