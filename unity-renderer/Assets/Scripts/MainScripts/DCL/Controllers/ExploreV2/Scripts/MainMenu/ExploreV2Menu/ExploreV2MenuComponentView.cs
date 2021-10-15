using System;
using UnityEngine;

public interface IExploreV2MenuComponentView : IDisposable
{
    /// <summary>
    /// It will be triggered when the view is fully initialized.
    /// </summary>
    event Action OnInitialized;

    /// <summary>
    /// It will be triggered when the close button is clicked.
    /// </summary>
    event Action OnCloseButtonPressed;

    /// <summary>
    /// Real viewer component.
    /// </summary>
    IRealmViewerComponentView currentRealmViewer { get; }

    /// <summary>
    /// Profile card component.
    /// </summary>
    IProfileCardComponentView currentProfileCard { get; }

    /// <summary>
    /// Places and Events section component.
    /// </summary>
    IPlacesAndEventsSectionComponentView currentPlacesAndEventsSection { get; }

    /// <summary>
    /// Returns true if the game object is activated.
    /// </summary>
    bool isActive { get; }

    /// <summary>
    /// Activates/Deactivates the game object of the explore menu.
    /// </summary>
    /// <param name="isActive">True to activate it.</param>
    void SetActive(bool isActive);
}

public class ExploreV2MenuComponentView : MonoBehaviour, IExploreV2MenuComponentView
{
    [Header("Top Menu")]
    [SerializeField] internal SectionSelectorComponentView sectionSelector;
    [SerializeField] internal ProfileCardComponentView profileCard;
    [SerializeField] internal RealmViewerComponentView realmViewer;
    [SerializeField] internal ButtonComponentView closeMenuButton;
    [SerializeField] internal InputAction_Trigger closeAction;

    [Header("Sections")]
    [SerializeField] internal PlacesAndEventsSectionComponentView placesAndEventsSection;

    public bool isActive => gameObject.activeSelf;

    public GameObject go => this != null ? gameObject : null;
    public IRealmViewerComponentView currentRealmViewer => realmViewer;
    public IProfileCardComponentView currentProfileCard => profileCard;
    public IPlacesAndEventsSectionComponentView currentPlacesAndEventsSection => placesAndEventsSection;

    public event Action OnInitialized;
    public event Action OnCloseButtonPressed;

    private void Start()
    {
        CreateSectionSelectorMappings();
        ConfigureCloseButton();

        OnInitialized?.Invoke();
    }

    private void OnDestroy()
    {
        RemoveSectionSelectorMappings();
        closeMenuButton.onClick.RemoveAllListeners();
    }

    public void SetActive(bool isActive) { gameObject.SetActive(isActive); }

    internal void CreateSectionSelectorMappings()
    {
        sectionSelector.GetSection(0)?.onSelect.AddListener((isOn) => placesAndEventsSection.gameObject.SetActive(isOn));

        ShowDefaultSection();
    }

    internal void RemoveSectionSelectorMappings() { sectionSelector.GetSection(0)?.onSelect.RemoveAllListeners(); }

    internal void ConfigureCloseButton()
    {
        closeMenuButton.onClick.AddListener(() => OnCloseButtonPressed?.Invoke());
        closeAction.OnTriggered += (action) => OnCloseButtonPressed?.Invoke();
    }

    internal void ShowDefaultSection() { placesAndEventsSection.gameObject.SetActive(true); }

    internal static ExploreV2MenuComponentView Create()
    {
        ExploreV2MenuComponentView exploreV2View = Instantiate(Resources.Load<GameObject>("MainMenu/ExploreV2Menu")).GetComponent<ExploreV2MenuComponentView>();
        exploreV2View.name = "_ExploreV2";

        return exploreV2View;
    }

    public void Dispose() { Destroy(gameObject); }
}