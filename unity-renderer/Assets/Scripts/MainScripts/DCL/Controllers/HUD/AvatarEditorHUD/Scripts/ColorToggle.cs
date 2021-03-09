using UnityEngine;
using UnityEngine.UI;

public class ColorToggle : UIButton
{
    public Color color
    {
        get;
        private set;
    }

    [SerializeField]
    private Image colorPicker;
    
    [SerializeField]
    private Image selectionHighlight;
    
    private bool selectedValue;

    public bool selected
    {
        get { return selectedValue; }
        set
        {
            selectedValue = value;
            selectionHighlight.enabled = selectedValue;
        }
    }
    
    public event System.Action<ColorToggle> OnClicked;

    private new void Awake()
    {
        base.Awake();
        Application.quitting += () =>
        {
            OnClicked = null;
        };
    }
    
    public void Initialize(Color c, bool on)
    {
        color = c;
        colorPicker.color = color;
        selected = on;
    }

    protected override void OnClick()
    {
        OnClicked?.Invoke(this);
    }
}