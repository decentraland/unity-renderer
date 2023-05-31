using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.Toggle;

public interface ISectionToggle
{
    GameObject GameObject { get; }
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

    /// <summary>
    /// Show/Hide the NEW tag.
    /// </summary>
    /// <param name="isNew">True for showing the tag.</param>
    void SetAsNew(bool isNew);
}

public class SectionToggle : MonoBehaviour, ISectionToggle, IPointerDownHandler
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private GameObject newTag;

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

    private void Awake() =>
        ConfigureDefaultOnSelectAction();

    private void OnEnable() =>
        StartCoroutine(ForceToRefreshToggleState());

    public void OnPointerDown(PointerEventData eventData) =>
        SelectToggle();

    public GameObject GameObject => gameObject;
    public RectTransform pivot => transform as RectTransform;
    public ToggleEvent onSelect => toggle != null ? toggle.onValueChanged : null;

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
            backgroundTransitionColorsForUnselected = backgroundTransitionColorsForUnselected,
            showNewTag = newTag != null && newTag.activeSelf,
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

        SetAsNew(model.showNewTag);

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

        if(!toggle.isOn)
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

    public void SetAsNew(bool isNew)
    {
        if (newTag == null)
            return;

        newTag.SetActive(isNew);
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

    internal IEnumerator ForceToRefreshToggleState()
    {
        // After each activation, in order to update the toggle's transition colors correctly, we need to force to change some property
        // of the component so that Unity notices it is in "dirty" state and it is refreshed.
        toggle.interactable = false;
        yield return null;
        toggle.interactable = true;
    }
}
