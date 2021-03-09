using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TaskbarButton : MonoBehaviour
{
    [FormerlySerializedAs("openButton")]
    public Button toggleButton;
    public GameObject lineOffIndicator;
    public ShowHideAnimator lineOnIndicator;

    public event System.Action<TaskbarButton> OnToggleOn;
    public event System.Action<TaskbarButton> OnToggleOff;

    public bool toggledOn { get; private set; } = true;

    public void Initialize()
    {
        toggleButton.onClick.AddListener(OnToggleButtonClick);
        SetToggleState(false, useCallback: false);
    }

    private void OnToggleButtonClick()
    {
        SetToggleState(!toggledOn);
    }

    public void SetToggleState(bool on, bool useCallback = true)
    {
        if (toggledOn == on)
            return;

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
}
