using UnityEngine;

public interface IExploreSectionComponentView
{
    /// <summary>
    /// Fill the model and updates the explore section with this data.
    /// </summary>
    /// <param name="model">Data to configure the explore section.</param>
    void Configure(ExploreSectionMenuComponentModel model);
}

public class ExploreSectionComponentView : BaseComponentView, IExploreSectionComponentView
{
    [Header("Top Menu")]
    [SerializeField] private SectionSelectorComponentView subSectionSelector;

    [Header("Sections")]
    [SerializeField] private HighlightsSubSectionComponentView highlightsSubSection;
    [SerializeField] private PlacesSubSectionComponentView placesSubSection;
    [SerializeField] private FavoritesSubSectionComponentView favoritesSubSection;
    [SerializeField] private MyPlacesSubSectionComponentView myPlacesSubSection;
    [SerializeField] private EventsSubSectionComponentView eventsSubSection;

    [Header("Configuration")]
    [SerializeField] protected ExploreSectionMenuComponentModel model;

    public override void Initialize()
    {
        base.Initialize();
        Configure(model);

        if (subSectionSelector.isInitialized)
            CreateSubSectionSelectorMappings();
        else
            subSectionSelector.OnInitialized += CreateSubSectionSelectorMappings;
    }

    public void Configure(ExploreSectionMenuComponentModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;
    }

    public override void Dispose()
    {
        subSectionSelector.OnInitialized -= CreateSubSectionSelectorMappings;
        RemoveSectionSelectorMappings();
    }

    internal void CreateSubSectionSelectorMappings()
    {
        subSectionSelector.GetSection(0)?.onSelect.AddListener((isOn) => highlightsSubSection.gameObject.SetActive(isOn));
        subSectionSelector.GetSection(1)?.onSelect.AddListener((isOn) => placesSubSection.gameObject.SetActive(isOn));
        subSectionSelector.GetSection(2)?.onSelect.AddListener((isOn) => favoritesSubSection.gameObject.SetActive(isOn));
        subSectionSelector.GetSection(3)?.onSelect.AddListener((isOn) => myPlacesSubSection.gameObject.SetActive(isOn));
        subSectionSelector.GetSection(4)?.onSelect.AddListener((isOn) => eventsSubSection.gameObject.SetActive(isOn));

        ShowDefaultSubSection();
    }

    internal void RemoveSectionSelectorMappings()
    {
        subSectionSelector.GetSection(0)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(1)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(2)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(3)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(4)?.onSelect.RemoveAllListeners();
    }

    internal void ShowDefaultSubSection() { highlightsSubSection.gameObject.SetActive(true); }
}