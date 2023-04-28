using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ColorToggle : MonoBehaviour
{
    [SerializeField] private Image colorPicker;
    [SerializeField] private Image selectionHighlight;

    private bool selectedValue;
    private Button button;

    public Color Color { get; private set; }

    public bool Selected
    {
        get => selectedValue;

        set
        {
            selectedValue = value;
            selectionHighlight.enabled = selectedValue;
        }
    }

    public event Action<ColorToggle> OnClicked;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => OnClicked?.Invoke(this));
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    public void Initialize(Color c, bool on)
    {
        Color = c;
        colorPicker.color = Color;
        Selected = on;
    }
}
