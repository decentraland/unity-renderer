using UnityEngine;

public interface IExploreV2MenuComponentView
{
    /// <summary>
    /// Fill the model and updates the explore menu with this data.
    /// </summary>
    /// <param name="model">Data to configure the explore menu.</param>
    void Configure(ExploreV2MenuComponentModel model);

    /// <summary>
    /// Set the realm info.
    /// </summary>
    /// <param name="realmInfo">RealmViewer model.</param>
    void SetRealmInfo(RealmViewerComponentModel realmInfo);

    /// <summary>
    /// Set the profile info.
    /// </summary>
    /// <param name="profileInfo">ProfileCard model.</param>
    void SetProfileInfo(ProfileCardComponentModel profileInfo);
}

public class ExploreV2MenuComponentView : BaseComponentView, IExploreV2MenuComponentView
{
    [Header("Top Menu")]
    [SerializeField] private SectionSelectorComponentView sectionSelector;
    [SerializeField] private ProfileCardComponentView profileCard;
    [SerializeField] private RealmViewerComponentView realmViewer;
    [SerializeField] private ButtonComponentView closeMenuButton;

    [Header("Sections")]
    [SerializeField] private ExploreSectionComponentView exploreSection;
    [SerializeField] private QuestSectionComponentView questSection;
    [SerializeField] private BackpackSectionComponentView backpackSection;
    [SerializeField] private MapSectionComponentView mapSection;
    [SerializeField] private BuilderSectionComponentView builderSection;
    [SerializeField] private MarketSectionComponentView marketSection;
    [SerializeField] private SettingsSectionComponentView settingsSection;

    [Header("Configuration")]
    [SerializeField] protected ExploreV2MenuComponentModel model;

    public override void Initialize()
    {
        base.Initialize();
        Configure(model);

        if (sectionSelector.isInitialized)
            CreateSectionSelectorMappings();
        else
            sectionSelector.OnInitialized += CreateSectionSelectorMappings;

        closeMenuButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    public void Configure(ExploreV2MenuComponentModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetRealmInfo(model.realmInfo);
        SetProfileInfo(model.profileInfo);
    }

    public override void Dispose()
    {
        sectionSelector.OnInitialized -= CreateSectionSelectorMappings;
        RemoveSectionSelectorMappings();
        closeMenuButton.onClick.RemoveAllListeners();
    }

    public void SetRealmInfo(RealmViewerComponentModel realmInfo) { realmViewer.Configure(realmInfo); }

    public void SetProfileInfo(ProfileCardComponentModel profileInfo) { profileCard.Configure(profileInfo); }

    internal void CreateSectionSelectorMappings()
    {
        sectionSelector.GetSection(0)?.onSelect.AddListener((isOn) => exploreSection.gameObject.SetActive(isOn));
        sectionSelector.GetSection(1)?.onSelect.AddListener((isOn) => questSection.gameObject.SetActive(isOn));
        sectionSelector.GetSection(2)?.onSelect.AddListener((isOn) => backpackSection.gameObject.SetActive(isOn));
        sectionSelector.GetSection(3)?.onSelect.AddListener((isOn) => mapSection.gameObject.SetActive(isOn));
        sectionSelector.GetSection(4)?.onSelect.AddListener((isOn) => builderSection.gameObject.SetActive(isOn));
        sectionSelector.GetSection(5)?.onSelect.AddListener((isOn) => marketSection.gameObject.SetActive(isOn));
        sectionSelector.GetSection(6)?.onSelect.AddListener((isOn) => settingsSection.gameObject.SetActive(isOn));

        ShowDefaultSection();
    }

    internal void RemoveSectionSelectorMappings()
    {
        sectionSelector.GetSection(0)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection(1)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection(2)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection(3)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection(4)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection(5)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection(6)?.onSelect.RemoveAllListeners();
    }

    internal void ShowDefaultSection() { exploreSection.gameObject.SetActive(true); }
}