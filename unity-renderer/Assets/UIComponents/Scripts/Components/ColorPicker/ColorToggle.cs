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

    public event System.Action<ColorToggle> OnClicked;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);

        Application.quitting += () =>
        {
            OnClicked = null;
        };
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClick);
    }

    public void Initialize(Color c, bool on)
    {
        Color = c;
        colorPicker.color = Color;
        Selected = on;
    }

    private void OnClick() =>
        OnClicked?.Invoke(this);
}
