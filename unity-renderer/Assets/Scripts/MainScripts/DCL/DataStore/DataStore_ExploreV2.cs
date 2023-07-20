using UnityEngine;

namespace DCL
{
    public class DataStore_ExploreV2
    {
        public readonly BaseVariable<bool> isInitialized = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> isPlacesAndEventsSectionInitialized = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> isOpen = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> isInShowAnimationTransiton = new BaseVariable<bool>(false);
        public readonly BaseVariable<int> currentSectionIndex = new BaseVariable<int>(0);
        public readonly BaseVariable<bool> placesAndEventsVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> profileCardIsOpen = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> isSomeModalOpen = new BaseVariable<bool>(false);
        public readonly BaseVariable<Transform> configureBackpackInFullscreenMenu = new BaseVariable<Transform>(null);
        public readonly BaseVariable<Transform> configureMapInFullscreenMenu = new BaseVariable<Transform>(null);
        public readonly BaseVariable<Transform> configureBuilderInFullscreenMenu = new BaseVariable<Transform>(null);
        public readonly BaseVariable<Transform> configureQuestInFullscreenMenu = new BaseVariable<Transform>(null);
        public readonly BaseVariable<Transform> configureSettingsInFullscreenMenu = new BaseVariable<Transform>(null);
        public readonly BaseVariable<Transform> configureWalletSectionInFullscreenMenu = new BaseVariable<Transform>(null);
        public readonly BaseVariable<Transform> configureMyAccountSectionInFullscreenMenu = new BaseVariable<Transform>(null);
        public readonly BaseVariable<Transform> topMenuTooltipReference = new BaseVariable<Transform>(null);
        public readonly BaseVariable<Transform> placesAndEventsTooltipReference = new BaseVariable<Transform>(null);
        public readonly BaseVariable<Transform> backpackTooltipReference = new BaseVariable<Transform>(null);
        public readonly BaseVariable<Transform> mapTooltipReference = new BaseVariable<Transform>(null);
        public readonly BaseVariable<Transform> builderTooltipReference = new BaseVariable<Transform>(null);
        public readonly BaseVariable<Transform> questTooltipReference = new BaseVariable<Transform>(null);
        public readonly BaseVariable<Transform> settingsTooltipReference = new BaseVariable<Transform>(null);
        public readonly BaseVariable<Transform> profileCardTooltipReference = new BaseVariable<Transform>(null);
        public readonly BaseVariable<ExploreV2CurrentModal> currentVisibleModal = new BaseVariable<ExploreV2CurrentModal>(ExploreV2CurrentModal.None);
    }
}
