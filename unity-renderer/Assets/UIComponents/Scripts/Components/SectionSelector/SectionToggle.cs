using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Toggle;

public interface ISectionToggle
{
    /// <summary>
    /// Event that will be triggered when the toggle is selected.
    /// </summary>
    ToggleEvent onSelect { get; }

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

    /// <summary>
    /// Set the toggle visuals as selected.
    /// </summary>
    void SetSelectedVisuals();

    /// <summary>
    /// Set the toggle visuals as unselected.
    /// </summary>
    void SetUnselectedVisuals();
}

public class SectionToggle : MonoBehaviour, ISectionToggle
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private Image toggleBackground;
    [SerializeField] private TMP_Text sectionText;
    [SerializeField] private Image sectionImage;
    [SerializeField] private Color selectedBackgroundColor;
    [SerializeField] private Color selectedTextColor;
    [SerializeField] private Color selectedImageColor;
    [SerializeField] private Color unselectedBackgroundColor;
    [SerializeField] private Color unselectedTextColor;
    [SerializeField] private Color unselectedImageColor;

    public ToggleEvent onSelect => toggle?.onValueChanged;

    private void Awake() { ConfigureDefaultOnSelectAction(); }

    public SectionToggleModel GetInfo()
    {
        return new SectionToggleModel
        {
            icon = sectionImage.sprite,
            title = sectionText.text
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

        selectedBackgroundColor = model.selectedBackgroundColor;
        selectedTextColor = model.selectedTextColor;
        selectedImageColor = model.selectedImageColor;
        unselectedBackgroundColor = model.unselectedBackgroundColor;
        unselectedTextColor = model.unselectedTextColor;
        unselectedImageColor = model.unselectedImageColor;

        onSelect.RemoveAllListeners();
        ConfigureDefaultOnSelectAction();
    }

    public void SelectToggle()
    {
        if (toggle == null)
            return;

        toggle.isOn = true;
    }

    public void SetSelectedVisuals()
    {
        toggleBackground.color = selectedBackgroundColor;
        sectionText.color = selectedTextColor;
        sectionImage.color = selectedImageColor;
    }

    public void SetUnselectedVisuals()
    {
        toggleBackground.color = unselectedBackgroundColor;
        sectionText.color = unselectedTextColor;
        sectionImage.color = unselectedImageColor;
    }

    internal void ConfigureDefaultOnSelectAction()
    {
        onSelect.AddListener((isOn) =>
        {
            if (isOn)
                SetSelectedVisuals();
            else
                SetUnselectedVisuals();
        });
    }
}