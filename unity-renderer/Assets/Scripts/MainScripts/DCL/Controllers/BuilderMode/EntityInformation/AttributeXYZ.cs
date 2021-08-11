using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AttributeXYZ : MonoBehaviour
{
    public TMP_InputField xField;
    public Image xTextBoxImage;
    public TMP_InputField yField;
    public Image yTextBoxImage;
    public TMP_InputField zField;
    public Image zTextBoxImage;

    public event Action<Vector3> OnChanged;

    Vector3 currentValue;

    bool isSelected = false;

    private void Awake()
    {
        ConfigureField(xField, xTextBoxImage, ChangeXValue);
        ConfigureField(yField, yTextBoxImage, ChangeYValue);
        ConfigureField(zField, zTextBoxImage, ChangeZValue);
    }

    private void OnDestroy()
    {
        UnsubscribeField(xField);
        UnsubscribeField(yField);
        UnsubscribeField(zField);
    }

    private void ConfigureField(TMP_InputField field, Image textBoxImage, UnityAction<string> onChangeAction)
    {
        if (field != null)
        {
            field.onValueChanged.AddListener(onChangeAction);

            field.onSelect.AddListener((currentText) =>
            {
                InputSelected(currentText);
                SetTextboxActive(textBoxImage, true);
            });

            field.onEndEdit.AddListener((newText) =>
            {
                SetTextboxActive(textBoxImage, false);
                InputDeselected(newText);

                if (EventSystem.current != null && !EventSystem.current.alreadySelecting)
                    EventSystem.current.SetSelectedGameObject(null);
            });

            field.onSubmit.AddListener((newText) => EventSystem.current?.SetSelectedGameObject(null));
        }

        SetTextboxActive(textBoxImage, false);
    }

    private void UnsubscribeField(TMP_InputField field)
    {
        if (field != null)
        {
            field.onValueChanged.RemoveAllListeners();
            field.onSelect.RemoveAllListeners();
            field.onEndEdit.RemoveAllListeners();
            field.onSubmit.RemoveAllListeners();
        }
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
        if (!isSelected || string.IsNullOrEmpty(value))
            return;

        value = value.Replace(".", ",");
        if (float.TryParse(value, out currentValue.x))
            OnChanged?.Invoke(currentValue);
    }

    public void ChangeYValue(string value)
    {
        if (!isSelected || string.IsNullOrEmpty(value))
            return;

        value = value.Replace(".", ",");
        if (float.TryParse(value, out currentValue.y))
            OnChanged?.Invoke(currentValue);
    }

    public void ChangeZValue(string value)
    {
        if (!isSelected || string.IsNullOrEmpty(value))
            return;

        value = value.Replace(".", ",");
        if (float.TryParse(value, out currentValue.z))
            OnChanged?.Invoke(currentValue);
    }

    public void InputSelected(string text) { isSelected = true; }

    public void InputDeselected(string text) { isSelected = false; }

    private void SetTextboxActive(Image textBoxImage, bool isActive)
    {
        if (textBoxImage == null)
            return;

        textBoxImage.enabled = isActive;
    }
}