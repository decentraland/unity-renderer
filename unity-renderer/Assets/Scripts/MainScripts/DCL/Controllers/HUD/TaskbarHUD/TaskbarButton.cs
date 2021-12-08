using DCL;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TaskbarButton : MonoBehaviour
{
    [FormerlySerializedAs("openButton")]
    public Button toggleButton;

    public GameObject lineOffIndicator;
    public ShowHideAnimator lineOnIndicator;
    public Image iconImage;
    public Color notInteractableColor;
    public List<AppMode> compatibleModes;
    public GameObject firstTimeLabelIndicator;

    public event System.Action<TaskbarButton> OnToggleOn;
    public event System.Action<TaskbarButton> OnToggleOff;

    public bool toggledOn { get; private set; } = true;

    private Color originalIconColor;

    public void Initialize()
    {
        toggleButton.onClick.RemoveAllListeners();
        toggleButton.onClick.AddListener(OnToggleButtonClick);
        SetToggleState(false, useCallback: false);

        if (iconImage != null)
            originalIconColor = iconImage.color;

        DataStore.i.common.appMode.OnChange += AppMode_OnChange;
        AppMode_OnChange(DataStore.i.common.appMode.Get(), AppMode.DEFAULT);
    }

    private void OnDestroy() { DataStore.i.common.appMode.OnChange -= AppMode_OnChange; }

    private void AppMode_OnChange(AppMode currentMode, AppMode previousMode)
    {
        bool isCompatible = compatibleModes.Contains(currentMode);

        SetInteractable(isCompatible);

        if (!isCompatible)
            SetToggleState(false);
    }

    private void OnToggleButtonClick() { SetToggleState(!toggledOn); }

    public void SetToggleState(bool on, bool useCallback = true)
    {
        if (toggledOn == on)
            return;

        if (on && firstTimeLabelIndicator != null)
            firstTimeLabelIndicator.SetActive(false);

        SetLineIndicator(on);

        if (!useCallback)
        {
            toggledOn = on;
            return;
        }

        if (on)
        {
            OnToggleOn?.Invoke(this);
            toggledOn = on;
        }
        else
        {
            toggledOn = on;
            OnToggleOff?.Invoke(this);
        }
    }

    public void SetLineIndicator(bool on)
    {
        if (lineOnIndicator != null)
        {
            if (on)
                lineOnIndicator.Show();
            else
                lineOnIndicator.Hide();
        }

        if (lineOffIndicator != null)
            lineOffIndicator.SetActive(!on);
    }

    private void SetInteractable(bool isInteractable)
    {
        toggleButton.interactable = isInteractable;

        if (iconImage != null)
            iconImage.color = isInteractable ? originalIconColor : notInteractableColor;
    }
}