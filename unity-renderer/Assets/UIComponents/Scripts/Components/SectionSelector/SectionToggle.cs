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
    [SerializeField] private TMP_Text sectionText;
    [SerializeField] private Image sectionImage;

    [Header("Visual Configuration When Selected")]
    [SerializeField] private ColorBlock backgroundTransitionColorsForSelected;
    [SerializeField] private Color selectedTextColor;
    [SerializeField] private Color selectedImageColor;

    [Header("Visual Configuration When Unselected")]
    [SerializeField] private ColorBlock backgroundTransitionColorsForUnselected;
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

        backgroundTransitionColorsForSelected = model.backgroundTransitionColorsForSelected;
        backgroundTransitionColorsForUnselected = model.backgroundTransitionColorsForUnselected;
        selectedTextColor = model.selectedTextColor;
        selectedImageColor = model.selectedImageColor;
        unselectedTextColor = model.unselectedTextColor;
        unselectedImageColor = model.unselectedImageColor;

        onSelect.RemoveAllListeners();
        ConfigureDefaultOnSelectAction();
        toggle.colors = model.backgroundTransitionColorsForSelected;
    }

    public void SelectToggle()
    {
        if (toggle == null)
            return;

        toggle.isOn = true;
    }

    public void SetSelectedVisuals()
    {
        toggle.colors = backgroundTransitionColorsForSelected;
        sectionText.color = selectedTextColor;
        sectionImage.color = selectedImageColor;
    }

    public void SetUnselectedVisuals()
    {
        toggle.colors = backgroundTransitionColorsForUnselected;
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