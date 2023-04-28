using System;
using UnityEngine;

public interface IExploreV2MenuComponentView : IDisposable
{

    /// <summary>
    /// Real viewer component.
    /// </summary>
    IRealmViewerComponentView currentRealmViewer { get; }

    /// <summary>
    /// Realm Selector component.
    /// </summary>
    IRealmSelectorComponentView currentRealmSelectorModal { get; }

    /// <summary>
    /// Profile card component.
    /// </summary>
    IProfileCardComponentView currentProfileCard { get; }

    /// <summary>
    /// Places and Events section component.
    /// </summary>
    IPlacesAndEventsSectionComponentView currentPlacesAndEventsSection { get; }

    /// <summary>
    /// Transform used to position the top menu tooltips.
    /// </summary>
    RectTransform currentTopMenuTooltipReference { get; }

    /// <summary>
    /// Transform used to position the places and events section tooltips.
    /// </summary>
    RectTransform currentPlacesAndEventsTooltipReference { get; }

    /// <summary>
    /// Transform used to position the backpack section tooltips.
    /// </summary>
    RectTransform currentBackpackTooltipReference { get; }

    /// <summary>
    /// Transform used to position the map section tooltips.
    /// </summary>
    RectTransform currentMapTooltipReference { get; }

    /// <summary>
    /// Transform used to position the builder section tooltips.
    /// </summary>

    /// <summary>
    /// Transform used to position the quest section tooltips.
    /// </summary>
    RectTransform currentQuestTooltipReference { get; }

    /// <summary>
    /// Transform used to position the settings section tooltips.
    /// </summary>
    RectTransform currentSettingsTooltipReference { get; }

    /// <summary>
    /// Transform used to position the profile section tooltips.
    /// </summary>
    RectTransform currentProfileCardTooltipReference { get; }
    /// <summary>
    /// It will be triggered when the close button is clicked.
    /// </summary>
    event Action<bool> OnCloseButtonPressed;

    /// <summary>
    /// It will be triggered when a section is open.
    /// </summary>
    event Action<ExploreSection> OnSectionOpen;

    /// <summary>
    /// It will be triggered after the show animation has finished.
    /// </summary>
    event Action OnAfterShowAnimation;

    /// <summary>
    /// Shows/Hides the game object of the explore menu.
    /// </summary>
    /// <param name="visible">True to show it.</param>
    void SetVisible(bool visible);

    /// <summary>
    /// Open a section.
    /// </summary>
    /// <param name="section">Section to go.</param>
    void GoToSection(ExploreSection section);

    /// <summary>
    /// Activates/Deactivates a section in the selector.
    /// </summary>
    /// <param name="section">Section to activate/deactivate.</param>
    /// <param name="isActive">True for activating.</param>
    void SetSectionActive(ExploreSection section, bool isActive);

    /// <summary>
    /// Check if a section is activated or not.
    /// </summary>
    /// <param name="section">Section to check.</param>
    /// <returns></returns>
    bool IsSectionActive(ExploreSection section);

    /// <summary>
    /// Configures a encapsulated section.
    /// </summary>
    /// <param name="sectionId">Section to configure.</param>
    /// <param name="featureConfiguratorFlag">Flag used to configure the feature.</param>
    void ConfigureEncapsulatedSection(ExploreSection sectionId, BaseVariable<Transform> featureConfiguratorFlag);

    /// <summary>
    /// Shows the Realm Selector modal.
    /// </summary>
    void ShowRealmSelectorModal();

    /// <summary>
    /// Hide Map section in UI when user enter isolated World
    /// </summary>
    void HideMapOnEnteringWorld();
}
