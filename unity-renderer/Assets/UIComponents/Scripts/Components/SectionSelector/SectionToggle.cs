using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Toggle;

public interface ISectionToggle
{
    /// <summary>
    /// Event that will be triggered when the toggle is selected.
    /// </summary>
    ToggleEvent onSelect { get; set; }

    /// <summary>
    /// Get the toggle info.
    /// </summary>
    /// <returns>Model with all the toggle info.</returns>
    SectionToggleModel GetInfo();

    /// <summary>
    /// Set the toggle info.
    /// </summary>
    /// <param name="model">Model with all the toggle info.</param>
    void SetInfo(SectionToggleModel model);

    /// <summary>
    /// Invoke the action of selecting the toggle.
    /// </summary>
    void SelectToggle();
}

public class SectionToggle : MonoBehaviour, ISectionToggle
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private TMP_Text sectionText;
    [SerializeField] private Image sectionImage;

    public ToggleEvent onSelect
    {
        get { return toggle?.onValueChanged; }
        set
        {
            toggle?.onValueChanged.RemoveAllListeners();
            toggle?.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                    value?.Invoke(isOn);
            });
        }
    }

    delegate void Logger(string str);

    public SectionToggleModel GetInfo()
    {
        return new SectionToggleModel
        {
            icon = sectionImage.sprite,
            title = sectionText.text,
            onSelectEvent = onSelect
        };
    }

    public void SetInfo(SectionToggleModel model)
    {
        if (model == null)
            return;

        if (sectionText != null)
            sectionText.text = model.title;

        if (sectionImage != null)
        {
            sectionImage.enabled = model.icon != null;
            sectionImage.sprite = model.icon;
        }

        onSelect = model.onSelectEvent;
    }

    public void SelectToggle()
    {
        if (toggle == null)
            return;

        toggle.isOn = true;
    }

    private void OnDestroy()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveAllListeners();
    }
}