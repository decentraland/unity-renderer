using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SpinBoxPresetted : MonoBehaviour
{
    [System.Serializable]
    public class OnValueChanged : UnityEvent<int>
    {
    }

    [Header("References")]
    [SerializeField] TMPro.TextMeshProUGUI textLabel = null;

    [SerializeField] Button increaseButton = null;
    [SerializeField] Button decreaseButton = null;

    [Header("Config")]
    [SerializeField] string[] labels = null;

    [SerializeField] int startingValue = 0;

    public bool loop;
    public OnValueChanged onValueChanged;

    private int currentValue;

    public int value
    {
        get
        {
            return currentValue;
        }
        set
        {
            SetValue(value);
        }
    }

    public string label
    {
        get { return textLabel.text; }
        set { OverrideCurrentLabel(value); }
    }

    void Awake()
    {
        increaseButton.onClick.AddListener(IncreaseValue);
        decreaseButton.onClick.AddListener(DecreaseValue);
        SetValue(startingValue, false);
    }

    public void SetLabels(string[] labels)
    {
        this.labels = labels;
    }

    public void SetValue(int value, bool raiseValueChangedEvent = true)
    {
        if (value >= labels.Length || value < 0)
        {
            return;
        }

        textLabel.text = labels[value];
        currentValue = (int)value;
        startingValue = currentValue;
        if (raiseValueChangedEvent) onValueChanged?.Invoke(value);

        if (!loop)
        {
            increaseButton.interactable = value < labels.Length - 1;
            decreaseButton.interactable = value > 0;
        }
    }

    public void OverrideCurrentLabel(string text)
    {
        textLabel.text = text;
    }

    public void IncreaseValue()
    {
        int newVal = currentValue + 1;

        if (newVal >= labels.Length)
        {
            if (loop)
                newVal = 0;
            else
                newVal = labels.Length - 1;
        }

        SetValue(newVal);
    }

    public void DecreaseValue()
    {
        int newVal = currentValue - 1;

        if (newVal < 0)
        {
            if (loop)
                newVal = labels.Length - 1;
            else
                newVal = 0;
        }

        SetValue(newVal);
    }
}