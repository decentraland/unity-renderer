using System.Collections;
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
    /// <param name="reselectIfAlreadyOn">True for apply the selection even if the toggle was already off.</param>
    void SelectToggle(bool reselectIfAlreadyOn = false);

    /// <summary>
    /// Set the toggle visuals as selected.
    /// </summary>
    void SetSelectedVisuals();

    /// <summary>
    /// Set the toggle visuals as unselected.
    /// </summary>
    void SetUnselectedVisuals();

    /// <summary>
    /// Set the toggle as active or inactive.
    /// </summary>
    /// <param name="isActive">Tru for activating.</param>
    void SetActive(bool isActive);

    /// <summary>
    /// Check if the toggle is active or not.
    /// </summary>
    /// <returns>True if it is actived.</returns>
    bool IsActive();
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

    private void OnEnable() { StartCoroutine(ForceToRefreshToggleState()); }

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
    }

    public void SelectToggle(bool reselectIfAlreadyOn = false)
    {
        if (toggle == null)
            return;

        if (reselectIfAlreadyOn)
            toggle.isOn = false;

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

    public void SetActive(bool isActive) { gameObject.SetActive(isActive); }

    public bool IsActive() { return gameObject.activeSelf; }

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

    internal IEnumerator ForceToRefreshToggleState()
    {
        // After each activation, in order to update the toggle's transition colors correctly, we need to force to change some property
        // of the component so that Unity notices it is in "dirty" state and it is refreshed.
        toggle.interactable = false;
        yield return null;
        toggle.interactable = true;
    }
}