using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.Toggle;

public interface ISectionToggle
{
    /// <summary>
    /// Pivot of the section object.
    /// </summary>
    RectTransform pivot { get; }

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

public class SectionToggle : MonoBehaviour, ISectionToggle, IPointerDownHandler
{
    [SerializeField] private Toggle toggle;

    [Header("Visual Configuration When Selected")]
    [SerializeField] private Image selectedIcon;
    [SerializeField] private TMP_Text selectedTitle;
    [SerializeField] private ColorBlock backgroundTransitionColorsForSelected;
    [SerializeField] private Color selectedTextColor;
    [SerializeField] private Color selectedImageColor;

    [Header("Visual Configuration When Unselected")]
    [SerializeField] private Image unselectedIcon;
    [SerializeField] private TMP_Text unselectedTitle;
    [SerializeField] private ColorBlock backgroundTransitionColorsForUnselected;
    [SerializeField] private Color unselectedTextColor;
    [SerializeField] private Color unselectedImageColor;

    public RectTransform pivot => transform as RectTransform;
    public ToggleEvent onSelect => toggle?.onValueChanged;

    private void Awake() { ConfigureDefaultOnSelectAction(); }

    private void OnEnable() { StartCoroutine(ForceToRefreshToggleState()); }

    public SectionToggleModel GetInfo()
    {
        return new SectionToggleModel
        {
            selectedIcon = selectedIcon.sprite,
            selectedTitle = selectedTitle.text,
            selectedTextColor = selectedTextColor,
            selectedImageColor = selectedImageColor,
            unselectedIcon = unselectedIcon.sprite,
            unselectedTitle = unselectedTitle.text,
            backgroundTransitionColorsForSelected = backgroundTransitionColorsForSelected,
            unselectedTextColor = unselectedTextColor,
            unselectedImageColor = unselectedImageColor,
            backgroundTransitionColorsForUnselected = backgroundTransitionColorsForUnselected
        };
    }

    public void SetInfo(SectionToggleModel model)
    {
        if (model == null)
            return;

        if (selectedTitle != null)
        {
            selectedTitle.text = model.selectedTitle;
            selectedTitle.color = model.selectedTextColor;
        }

        if (unselectedTitle != null)
        {
            unselectedTitle.text = model.unselectedTitle;
            unselectedTitle.color = model.unselectedTextColor;
        }

        if (selectedIcon != null)
        {
            selectedIcon.sprite = model.selectedIcon;
            selectedIcon.color = model.selectedImageColor;
        }

        if (unselectedIcon != null)
        {
            unselectedIcon.sprite = model.unselectedIcon;
            unselectedIcon.color = model.unselectedImageColor;
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

    public void OnPointerDown(PointerEventData eventData)
    {
        SelectToggle();
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
        if (selectedIcon != null)
            selectedIcon.gameObject.SetActive(true);

        if (unselectedIcon != null)
            unselectedIcon.gameObject.SetActive(false);

        if (selectedTitle != null)
            selectedTitle.gameObject.SetActive(true);

        if (unselectedTitle != null)
            unselectedTitle.gameObject.SetActive(false);

        toggle.colors = backgroundTransitionColorsForSelected;
    }

    public void SetUnselectedVisuals()
    {
        if (selectedIcon != null)
            selectedIcon.gameObject.SetActive(false);

        if (unselectedIcon != null)
            unselectedIcon.gameObject.SetActive(true);

        if (selectedTitle != null)
            selectedTitle.gameObject.SetActive(false);

        if (unselectedTitle != null)
            unselectedTitle.gameObject.SetActive(true);

        toggle.colors = backgroundTransitionColorsForUnselected;
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